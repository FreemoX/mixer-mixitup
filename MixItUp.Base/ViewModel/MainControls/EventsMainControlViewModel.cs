﻿using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class EventCommandItemViewModel
    {
        public EventTypeEnum EventType { get; set; }

        public EventCommandModel Command { get; set; }

        public EventCommandItemViewModel(EventCommandModel command)
        {
            this.Command = command;
            this.EventType = this.Command.EventType;
        }

        public EventCommandItemViewModel(EventTypeEnum eventType) { this.EventType = eventType; }

        public string Name { get { return EnumLocalizationHelper.GetLocalizedName(this.EventType); } }

        public string Service
        {
            get
            {
                int eventNumber = (int)this.EventType;
                if (this.EventType == EventTypeEnum.StreamlabsDonation)
                {
                    return Resources.Streamlabs;
                }
                else if (this.EventType == EventTypeEnum.TiltifyDonation)
                {
                    return Resources.Tiltify;
                }
                else if (this.EventType == EventTypeEnum.ExtraLifeDonation)
                {
                    return Resources.ExtraLife;
                }
                else if (this.EventType == EventTypeEnum.TipeeeStreamDonation)
                {
                    return Resources.TipeeeStream;
                }
                else if (this.EventType == EventTypeEnum.TreatStreamDonation)
                {
                    return Resources.TreatStream;
                }
                else if (this.EventType == EventTypeEnum.RainmakerDonation)
                {
                    return Resources.Rainmaker;
                }
                else if (this.EventType == EventTypeEnum.PatreonSubscribed)
                {
                    return Resources.Patreon;
                }
                else if (this.EventType == EventTypeEnum.JustGivingDonation)
                {
                    return Resources.JustGiving;
                }
                else if (this.EventType == EventTypeEnum.StreamlootsCardRedeemed || this.EventType == EventTypeEnum.StreamlootsPackGifted || this.EventType == EventTypeEnum.StreamlootsPackPurchased)
                {
                    return Resources.Streamloots;
                }
                else if (this.EventType == EventTypeEnum.StreamElementsDonation || this.EventType == EventTypeEnum.StreamElementsMerchPurchase)
                {
                    return Resources.StreamElements;
                }
                else if (eventNumber >= 100 && eventNumber < 200)
                {
                    return "Mixer";
                }
                else if (eventNumber >= 200 && eventNumber < 300)
                {
                    return Resources.Twitch;
                }
                else
                {
                    return Resources.Generic;
                }
            }
        }

        public bool IsNewCommand { get { return this.Command == null; } }

        public bool IsExistingCommand { get { return this.Command != null; } }
    }

    public class EventsMainControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<EventCommandItemViewModel> EventCommands { get; set; } = new ThreadSafeObservableCollection<EventCommandItemViewModel>();

        public EventsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.RefreshCommands();
        }

        public void RefreshCommands()
        {
            this.EventCommands.Clear();

            List<EventCommandItemViewModel> commands = new List<EventCommandItemViewModel>();

            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelStreamStart));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelStreamStop));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelFollowed));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelHosted));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelRaided));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelSubscribed));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelResubscribed));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelSubscriptionGifted));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelMassSubscriptionsGifted));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelBitsCheered));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelPointsRedeemed));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelHypeTrainBegin));
            commands.Add(this.GetEventCommand(EventTypeEnum.TwitchChannelHypeTrainEnd));

            commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserFirstJoin));
            commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserJoined));
            commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserLeft));
            //commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserPurge));
            commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserTimeout));
            commands.Add(this.GetEventCommand(EventTypeEnum.ChatUserBan));
            commands.Add(this.GetEventCommand(EventTypeEnum.ChatMessageReceived));
            commands.Add(this.GetEventCommand(EventTypeEnum.ChatWhisperReceived));
            commands.Add(this.GetEventCommand(EventTypeEnum.ChatMessageDeleted));

            commands.Add(this.GetEventCommand(EventTypeEnum.StreamlabsDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.StreamElementsDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.StreamElementsMerchPurchase));
            commands.Add(this.GetEventCommand(EventTypeEnum.TipeeeStreamDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.TreatStreamDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.RainmakerDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.TiltifyDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.ExtraLifeDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.JustGivingDonation));
            commands.Add(this.GetEventCommand(EventTypeEnum.PatreonSubscribed));
            commands.Add(this.GetEventCommand(EventTypeEnum.StreamlootsCardRedeemed));
            commands.Add(this.GetEventCommand(EventTypeEnum.StreamlootsPackPurchased));
            commands.Add(this.GetEventCommand(EventTypeEnum.StreamlootsPackGifted));

            this.EventCommands.AddRange(commands);
        }

        private EventCommandItemViewModel GetEventCommand(EventTypeEnum eventType)
        {
            EventCommandModel command = ChannelSession.Services.Events.GetEventCommand(eventType);
            if (command != null)
            {
                return new EventCommandItemViewModel(command);
            }
            else
            {
                return new EventCommandItemViewModel(eventType);
            }
        }
    }
}
