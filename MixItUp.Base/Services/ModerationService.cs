﻿using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum ModerationChatInteractiveParticipationEnum
    {
        None = 0,
        AccountHour = 1,
        AccountDay = 2,
        AccountWeek = 3,
        AccountMonth = 4,
        ViewingTenMinutes = 10,
        ViewingThirtyMinutes = 11,
        ViewingOneHour = 12,
        ViewingTwoHours = 13,
        ViewingTenHours = 14,
        FollowerOnly = 19,
        SubscriberOnly = 20,
        ModeratorOnly = 30,
        [Obsolete]
        EmotesSkillsOnly = 40,
        [Obsolete]
        SkillsOnly = 41,
        [Obsolete]
        EmberSkillsOnly = 42,
    }

    public interface IModerationService
    {
        Task Initialize();
        void RebuildCache();

        Task<string> ShouldTextBeModerated(UserViewModel user, string text, bool containsLink = false);
        Task<string> ShouldTextBeFilteredWordModerated(UserViewModel user, string text);
        string ShouldTextBeExcessiveModerated(UserViewModel user, string text);
        string ShouldTextBeLinkModerated(UserViewModel user, string text, bool containsLink = false);

        bool DoesUserMeetChatInteractiveParticipationRequirement(UserViewModel user, ChatMessageViewModel message = null);
        Task SendChatInteractiveParticipationWhisper(UserViewModel user, bool isChat = false);
    }

    public class ModerationService : IModerationService
    {
        public const string ModerationReasonSpecialIdentifier = "moderationreason";

        public const string WordRegexFormat = "(^|[^\\w]){0}([^\\w]|$)";
        public const string WordWildcardRegex = "\\S*";
        public static readonly string WordWildcardRegexEscaped = Regex.Escape(WordWildcardRegex);

        public static LockedList<string> CommunityFilteredWords { get; set; } = new LockedList<string>();

        private const string CommunityFilteredWordsFilePath = "Assets\\CommunityBannedWords.txt";

        private const int MinimumMessageLengthForPercentageModeration = 5;

        private static readonly Regex EmoteRegex = new Regex(":\\w+ ");
        private static readonly Regex EmojiRegex = new Regex(@"\uD83D[\uDC00-\uDFFF]|\uD83C[\uDC00-\uDFFF]|\uFFFD");
        private static readonly Regex LinkRegex = new Regex(@"(?xi)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))");

        private LockedList<string> communityWords = new LockedList<string>();
        private LockedList<string> filteredWords = new LockedList<string>();
        private LockedList<string> bannedWords = new LockedList<string>();

        private DateTimeOffset chatParticipationLastErrorMessage = DateTimeOffset.MinValue;

        public async Task Initialize()
        {
            if (ChannelSession.Services.FileService.FileExists(ModerationService.CommunityFilteredWordsFilePath))
            {
                string text = await ChannelSession.Services.FileService.ReadFile(ModerationService.CommunityFilteredWordsFilePath);
                ModerationService.CommunityFilteredWords = new LockedList<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

                foreach (string word in ModerationService.CommunityFilteredWords)
                {
                    this.communityWords.Add(string.Format(WordRegexFormat, Regex.Escape(word)));
                }
            }

            this.RebuildCache();
        }

        public void RebuildCache()
        {
            this.filteredWords.Clear();
            foreach (string word in ChannelSession.Settings.FilteredWords)
            {
                this.filteredWords.Add(string.Format(WordRegexFormat, Regex.Escape(word).Replace(WordWildcardRegexEscaped, WordWildcardRegex)));
            }

            this.bannedWords.Clear();
            foreach (string word in ChannelSession.Settings.BannedWords)
            {
                this.bannedWords.Add(string.Format(WordRegexFormat, Regex.Escape(word).Replace(WordWildcardRegexEscaped, WordWildcardRegex)));
            }
        }

        public async Task<string> ShouldTextBeModerated(UserViewModel user, string text, bool containsLink = false)
        {
            string reason = null;

            if (string.IsNullOrEmpty(text) || user.IgnoreForQueries)
            {
                return reason;
            }

            reason = await ShouldTextBeFilteredWordModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationFilteredWordsApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            reason = ShouldTextBeExcessiveModerated(user, text);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationChatTextApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            reason = ShouldTextBeLinkModerated(user, text, containsLink);
            if (!string.IsNullOrEmpty(reason))
            {
                if (ChannelSession.Settings.ModerationBlockLinksApplyStrikes)
                {
                    await user.AddModerationStrike(reason);
                }
                return reason;
            }

            return reason;
        }

        public async Task<string> ShouldTextBeFilteredWordModerated(UserViewModel user, string text)
        {
            text = PrepareTextForChecking(text);

            if (!user.HasPermissionsTo(ChannelSession.Settings.ModerationFilteredWordsExcempt))
            {
                if (ChannelSession.Settings.ModerationUseCommunityFilteredWords)
                {
                    foreach (string word in this.communityWords)
                    {
                        if (Regex.IsMatch(text, word, RegexOptions.IgnoreCase))
                        {
                            return "The previous message was deleted due to a filtered word";
                        }
                    }
                }

                foreach (string word in this.filteredWords)
                {
                    if (Regex.IsMatch(text, word, RegexOptions.IgnoreCase))
                    {
                        return "The previous message was deleted due to a filtered word";
                    }
                }

                foreach (string word in this.bannedWords)
                {
                    if (Regex.IsMatch(text, word, RegexOptions.IgnoreCase))
                    {
                        await ChannelSession.Services.Chat.BanUser(user);
                        return "The previous message was deleted due to a banned Word";
                    }
                }
            }

            return null;
        }

        public string ShouldTextBeExcessiveModerated(UserViewModel user, string text)
        {
            if (!user.HasPermissionsTo(ChannelSession.Settings.ModerationChatTextExcempt))
            {
                if (ChannelSession.Settings.ModerationCapsBlockCount > 0)
                {
                    int count = text.Count(c => char.IsUpper(c));
                    if (ChannelSession.Settings.ModerationCapsBlockIsPercentage)
                    {
                        count = ConvertCountToPercentage(text.Count(), count);
                    }

                    if (count >= ChannelSession.Settings.ModerationCapsBlockCount)
                    {
                        return "Too Many Caps";
                    }
                }

                // Perform text preparing after checking for caps
                text = PrepareTextForChecking(text);

                if (ChannelSession.Settings.ModerationPunctuationBlockCount > 0)
                {
                    string leftOverText = text.ToString();
                    List<string> messageSegments = new List<string>();
                    int count = 0;

                    foreach (Match match in EmoteRegex.Matches(text))
                    {
                        messageSegments.Add(match.Value);
                        leftOverText = leftOverText.Replace(match.Value, "");
                        count++;
                    }
                    foreach (Match match in EmojiRegex.Matches(text))
                    {
                        messageSegments.Add(match.Value);
                        leftOverText = leftOverText.Replace(match.Value, "");
                        count++;
                    }

                    if (!string.IsNullOrEmpty(leftOverText))
                    {
                        count += leftOverText.Count(c => char.IsSymbol(c) || char.IsPunctuation(c));
                        messageSegments.AddRange(leftOverText.ToCharArray().Select(c => c.ToString()));
                    }

                    if (ChannelSession.Settings.ModerationPunctuationBlockIsPercentage)
                    {
                        count = ConvertCountToPercentage(messageSegments.Count, count);
                    }

                    if (count >= ChannelSession.Settings.ModerationPunctuationBlockCount)
                    {
                        return "Too Many Punctuation/Symbols/Emotes";
                    }
                }
            }

            return null;
        }

        public string ShouldTextBeLinkModerated(UserViewModel user, string text, bool containsLink = false)
        {
            text = PrepareTextForChecking(text);

            if (!user.HasPermissionsTo(ChannelSession.Settings.ModerationBlockLinksExcempt))
            {
                if (ChannelSession.Settings.ModerationBlockLinks && (containsLink || LinkRegex.IsMatch(text)))
                {
                    return "No Links";
                }
            }

            return null;
        }

        public bool DoesUserMeetChatInteractiveParticipationRequirement(UserViewModel user, ChatMessageViewModel message = null)
        {
            if (ChannelSession.Settings.ModerationChatInteractiveParticipation != ModerationChatInteractiveParticipationEnum.None)
            {
                if (user == null)
                {
                    return false;
                }

                if (user.IgnoreForQueries)
                {
                    return true;
                }

                if (user.HasPermissionsTo(ChannelSession.Settings.ModerationChatInteractiveParticipationExcempt))
                {
                    return true;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.FollowerOnly && !user.HasPermissionsTo(UserRoleEnum.Follower))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SubscriberOnly && !user.HasPermissionsTo(UserRoleEnum.Subscriber))
                {
                    return false;
                }

                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ModeratorOnly && !user.HasPermissionsTo(UserRoleEnum.Mod))
                {
                    return false;
                }

                if (user.AccountDate.HasValue)
                {
                    TimeSpan accountLength = DateTimeOffset.Now - user.AccountDate.GetValueOrDefault();
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountHour && accountLength.TotalHours < 1)
                    {
                        return false;
                    }
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountDay && accountLength.TotalDays < 1)
                    {
                        return false;
                    }
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountWeek && accountLength.TotalDays < 7)
                    {
                        return false;
                    }
                    if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountMonth && accountLength.TotalDays < 30)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                TimeSpan viewingLength = TimeSpan.FromMinutes(user.Data.ViewingMinutes);
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenMinutes && viewingLength.TotalMinutes < 10)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingThirtyMinutes && viewingLength.TotalMinutes < 30)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingOneHour && viewingLength.TotalHours < 1)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTwoHours && viewingLength.TotalHours < 2)
                {
                    return false;
                }
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenHours && viewingLength.TotalHours < 10)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task SendChatInteractiveParticipationWhisper(UserViewModel user, bool isChat = false)
        {
            if (user != null)
            {
                string reason = string.Empty;
                if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.FollowerOnly)
                {
                    reason = "Followers";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.SubscriberOnly)
                {
                    reason = "Subscribers";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ModeratorOnly)
                {
                    reason = "Moderators";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountHour)
                {
                    reason = "accounts older than 1 hour";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountDay)
                {
                    reason = "accounts older than 1 day";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountWeek)
                {
                    reason = "accounts older than 1 week";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.AccountMonth)
                {
                    reason = "accounts older than 1 month";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenMinutes)
                {
                    reason = "viewers who have watched for 10 minutes";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingThirtyMinutes)
                {
                    reason = "viewers who have watched for 30 minutes";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingOneHour)
                {
                    reason = "viewers who have watched for 1 hour";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTwoHours)
                {
                    reason = "viewers who have watched for 2 hours";
                }
                else if (ChannelSession.Settings.ModerationChatInteractiveParticipation == ModerationChatInteractiveParticipationEnum.ViewingTenHours)
                {
                    reason = "viewers who have watched for 10 hours";
                }

                if (isChat)
                {
                    if (this.chatParticipationLastErrorMessage > DateTimeOffset.Now)
                    {
                        return;
                    }

                    this.chatParticipationLastErrorMessage = DateTimeOffset.Now.AddSeconds(10);
                    await ChannelSession.Services.Chat.SendMessage(string.Format("@{0}: Your message has been deleted because only {1} can participate currently.", user.Username, reason), platform: user.Platform);
                }
            }
        }

        private string PrepareTextForChecking(string text)
        {
            string result = string.IsNullOrEmpty(text) ? string.Empty : text.ToLower();
            result = ChatListControlViewModel.UserNameTagRegex.Replace(result, "");
            return result;
        }

        private int ConvertCountToPercentage(int length, int count)
        {
            if (length >= MinimumMessageLengthForPercentageModeration)
            {
                return (int)(((double)count) / ((double)length) * 100.0);
            }
            else
            {
                return 0;
            }
        }
    }
}
