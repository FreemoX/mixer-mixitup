﻿using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Controls.Commands.Games;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Commands
{
    /// <summary>
    /// Interaction logic for GameCommandEditorWindow.xaml
    /// </summary>
    public partial class GameCommandEditorWindow : LoadingWindowBase
    {
        public CommandEditorDetailsControlBase editorDetailsControl { get; private set; }

        public GameCommandEditorWindowViewModelBase viewModel { get; private set; }

        public event EventHandler<CommandModelBase> CommandSaved = delegate { };

        public GameCommandEditorWindow(GameCommandModelBase existingCommand)
            : this()
        {
            switch (existingCommand.GameType)
            {
                case GameCommandTypeEnum.BeachBall:
                    break;
                case GameCommandTypeEnum.Bet:
                    break;
                case GameCommandTypeEnum.Bid:
                    break;
                case GameCommandTypeEnum.CoinPusher:
                    break;
                case GameCommandTypeEnum.Duel:
                    break;
                case GameCommandTypeEnum.Hangman:
                    break;
                case GameCommandTypeEnum.Heist:
                    break;
                case GameCommandTypeEnum.Hitman:
                    break;
                case GameCommandTypeEnum.HotPotato:
                    break;
                case GameCommandTypeEnum.LockBox:
                    break;
                case GameCommandTypeEnum.Roulette:
                    break;
                case GameCommandTypeEnum.RussianRoulette:
                    break;
                case GameCommandTypeEnum.SlotMachine:
                    break;
                case GameCommandTypeEnum.Spin:
                    this.editorDetailsControl = new SpinGameCommandEditorDetailsControl();
                    this.viewModel = new SpinGameCommandEditorWindowViewModel((SpinGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Steal:
                    break;
                case GameCommandTypeEnum.TreasureDefense:
                    break;
                case GameCommandTypeEnum.Trivia:
                    break;
                case GameCommandTypeEnum.Volcano:
                    break;
                case GameCommandTypeEnum.WordScramble:
                    break;
            }

            this.DataContext = this.ViewModel = this.viewModel;
            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        public GameCommandEditorWindow(GameCommandTypeEnum gameType, CurrencyModel currency)
            : this()
        {
            switch (gameType)
            {
                case GameCommandTypeEnum.BeachBall:
                    break;
                case GameCommandTypeEnum.Bet:
                    break;
                case GameCommandTypeEnum.Bid:
                    break;
                case GameCommandTypeEnum.CoinPusher:
                    break;
                case GameCommandTypeEnum.Duel:
                    break;
                case GameCommandTypeEnum.Hangman:
                    break;
                case GameCommandTypeEnum.Heist:
                    break;
                case GameCommandTypeEnum.Hitman:
                    break;
                case GameCommandTypeEnum.HotPotato:
                    break;
                case GameCommandTypeEnum.LockBox:
                    break;
                case GameCommandTypeEnum.Roulette:
                    break;
                case GameCommandTypeEnum.RussianRoulette:
                    break;
                case GameCommandTypeEnum.SlotMachine:
                    break;
                case GameCommandTypeEnum.Spin:
                    this.editorDetailsControl = new SpinGameCommandEditorDetailsControl();
                    this.viewModel = new SpinGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Steal:
                    break;
                case GameCommandTypeEnum.TreasureDefense:
                    break;
                case GameCommandTypeEnum.Trivia:
                    break;
                case GameCommandTypeEnum.Volcano:
                    break;
                case GameCommandTypeEnum.WordScramble:
                    break;
            }

            this.DataContext = this.ViewModel = this.viewModel;
            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        private GameCommandEditorWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            if (this.viewModel != null)
            {
                this.viewModel.CommandSaved += ViewModel_CommandSaved;
                await this.viewModel.OnLoaded();

                this.DetailsContentControl.Content = this.editorDetailsControl;
            }
            await base.OnLoaded();
        }

        private void ViewModel_CommandSaved(object sender, CommandModelBase command)
        {
            this.CommandSaved(this, command);
            this.Close();
        }
    }
}