﻿using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class StreamlootsCardsMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public ICommand StreamlootsManageCollectionCommand { get; set; }

        public StreamlootsCardsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited += GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited;

            this.StreamlootsManageCollectionCommand = this.CreateCommand((parameters) =>
            {
                if (ChannelSession.TwitchUserConnection != null)
                {
                    ProcessHelper.LaunchLink($"https://www.streamloots.com/{ChannelSession.TwitchUserNewAPI.login}/manage/cards");
                }
            });
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.Services.Command.StreamlootsCardCommands.ToList();
        }

        private void GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited(object sender, CommandModelBase command)
        {
            if (command.Type == CommandTypeEnum.StreamlootsCard)
            {
                this.AddCommand(command);
            }
        }
    }
}
