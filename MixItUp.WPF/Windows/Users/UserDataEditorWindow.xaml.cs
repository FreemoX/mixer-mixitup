﻿using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Users
{
    /// <summary>
    /// Interaction logic for UserDataEditorWindow.xaml
    /// </summary>
    public partial class UserDataEditorWindow : LoadingWindowBase
    {
        private UserViewModel user;
        private UserDataEditorWindowViewModel viewModel;

        public UserDataEditorWindow(UserDataModel userData)
        {
            this.user = new UserViewModel(userData);
            this.DataContext = this.viewModel = new UserDataEditorWindowViewModel(userData);

            InitializeComponent();

            this.Initialize(this.StatusBar);

            this.Closed += UserDataEditorWindow_Closed;
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.Load();
        }

        private void AddUserOnlyCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.UserOnlyChat, this.viewModel.User.ID);
            window.CommandSaved += (object s, CommandModelBase command) =>
            {
                this.viewModel.AddUserOnlyChatCommand((UserOnlyChatCommandModel)command);
                this.viewModel.RefreshUserOnlyChatCommands();
            };
            window.ForceShow();
        }

        private void UserOnlyChatCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<UserOnlyChatCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.RefreshUserOnlyChatCommands(); };
            window.ForceShow();
        }

        private async void UserOnlyChatCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                this.viewModel.RemoveUserOnlyChatCommand(FrameworkElementHelpers.GetDataContext<UserOnlyChatCommandModel>(sender));
                await ChannelSession.SaveSettings();
                ChannelSession.Services.Chat.RebuildCommandTriggers();
            });
        }

        private void UserOnlyChatCommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            ChannelSession.Services.Chat.RebuildCommandTriggers();
        }

        private void NewEntranceCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, UserDataEditorWindowViewModel.UserEntranceCommandName);
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.EntranceCommand = command; };
            window.ForceShow();
        }

        private void ExistingEntranceCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CommandModelBase>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.EntranceCommand = command; };
            window.ForceShow();
        }

        private async void ExistingEntranceCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                this.viewModel.EntranceCommand = null;
                await ChannelSession.SaveSettings();
            });
        }

        private void UserDataEditorWindow_Closed(object sender, EventArgs e)
        {
            ChannelSession.Settings.UserData.ManualValueChanged(this.user.ID);
        }
    }
}
