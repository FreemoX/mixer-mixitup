﻿using MixItUp.Base.Model.Commands;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class ActionGroupMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public ActionGroupMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited += GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited;
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.Services.Command.ActionGroupCommands.ToList();
        }

        private void GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited(object sender, CommandModelBase command)
        {
            if (command.Type == CommandTypeEnum.ActionGroup)
            {
                this.AddCommand(command);
            }
        }
    }
}
