﻿using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Windows.Users;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using MixItUp.Base.Util;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for UsersControl.xaml
    /// </summary>
    public partial class UsersControl : MainControlBase
    {
        private UsersMainControlViewModel viewModel;
        private Timer textChangedTimer;

        public UsersControl()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            textChangedTimer = new Timer((e) => UpdateText(), null, Timeout.Infinite, Timeout.Infinite);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new UsersMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private async Task UpdateText()
        {
            await this.viewModel.RefreshUsersAsync();
            await DispatcherHelper.Dispatcher.InvokeAsync(() =>
            {
                this.UsernameFilterTextBox.Focus();
                return Task.CompletedTask;
            });
        }

        private void UsernameFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.viewModel.UsernameFilter = this.UsernameFilterTextBox.Text;
            textChangedTimer.Change(500, Timeout.Infinite);
        }

        private void FilterUsersButton_Click(object sender, RoutedEventArgs e)
        {
            textChangedTimer.Change(1, Timeout.Infinite);
        }

        private void UserEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataModel userData = (UserDataModel)button.DataContext;
            UserDataEditorWindow window = new UserDataEditorWindow(userData);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void UserDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataModel userData = (UserDataModel)button.DataContext;
            await this.viewModel.DeleteUser(userData);
        }

        private void UserDataGridView_Sorted(object sender, DataGridColumn column)
        {
            this.viewModel.SetSortColumnIndexAndDirection(this.UserDataGridView.Columns.IndexOf(column), column.SortDirection.GetValueOrDefault());
        }

        private void ImportUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            UserDataImportWindow window = new UserDataImportWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.RefreshUsers();
        }
    }
}
