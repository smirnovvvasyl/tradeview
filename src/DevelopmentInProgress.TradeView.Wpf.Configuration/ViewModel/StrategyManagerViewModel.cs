﻿using DevelopmentInProgress.TradeView.Wpf.Common.Events;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using DevelopmentInProgress.TradeView.Wpf.Common.Services;
using DevelopmentInProgress.TradeView.Wpf.Configuration.Utility;
using DevelopmentInProgress.TradeView.Wpf.Controls.Messaging;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.Context;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace DevelopmentInProgress.TradeView.Wpf.Configuration.ViewModel
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "General exceptions are written to the log file and show in the messages panel.")]
    public class StrategyManagerViewModel : DocumentViewModel
    {
        private readonly IStrategyService strategyService;
        private readonly IStrategyFileManager strategyFileManager;
        private readonly Dictionary<string, IDisposable> strategyObservableSubscriptions;
        private StrategyViewModel selectedStrategyViewModel;
        private Strategy selectedStrategy;
        private bool isLoading;
        private bool disposed;

        public StrategyManagerViewModel(ViewModelContext viewModelContext, IStrategyService strategyService, IStrategyFileManager strategyFileManager)
            : base(viewModelContext)
        {
            this.strategyService = strategyService;
            this.strategyFileManager = strategyFileManager;

            AddStrategyCommand = new ViewModelCommand(AddStrategy);
            DeleteStrategyCommand = new ViewModelCommand(DeleteStrategy);
            CloseCommand = new ViewModelCommand(Close);

            Strategies = new ObservableCollection<Strategy>();
            SelectedStrategyViewModels = new ObservableCollection<StrategyViewModel>();

            strategyObservableSubscriptions = new Dictionary<string, IDisposable>();
        }

        public ICommand AddStrategyCommand { get; set; }
        public ICommand DeleteStrategyCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public ObservableCollection<Strategy> Strategies { get; }

        public ObservableCollection<StrategyViewModel> SelectedStrategyViewModels { get; }

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public Strategy SelectedStrategy
        {
            get { return selectedStrategy; }
            set
            {
                if (selectedStrategy != value)
                {
                    selectedStrategy = value;

                    if (selectedStrategy != null)
                    {
                        var strategyViewModel = SelectedStrategyViewModels.FirstOrDefault(s => s.Strategy.Name.Equals(selectedStrategy.Name, StringComparison.Ordinal));

                        if (strategyViewModel == null)
                        {
                            strategyViewModel = new StrategyViewModel(selectedStrategy, strategyService, strategyFileManager, Logger);
                            ObserveStrategy(strategyViewModel);
                            SelectedStrategyViewModels.Add(strategyViewModel);
                            SelectedStrategyViewModel = strategyViewModel;
                        }
                        else
                        {
                            SelectedStrategyViewModel = strategyViewModel;
                        }
                    }

                    OnPropertyChanged(nameof(SelectedStrategy));
                }
            }
        }

        public StrategyViewModel SelectedStrategyViewModel
        {
            get { return selectedStrategyViewModel; }
            set
            {
                if (selectedStrategyViewModel != value)
                {
                    selectedStrategyViewModel = value;
                    OnPropertyChanged(nameof(SelectedStrategyViewModel));
                }
            }
        }

        public void Close(object param)
        {
            if (param is StrategyViewModel strategy)
            {
                strategy.Dispose();

                if (strategyObservableSubscriptions.TryGetValue(strategy.Strategy.Name, out IDisposable subscription))
                {
                    subscription.Dispose();
                }

                strategyObservableSubscriptions.Remove(strategy.Strategy.Name);
                
                SelectedStrategyViewModels.Remove(strategy);
            }
        }

        protected async override void OnPublished(object data)
        {
            IsBusy = true;

            base.OnPublished(data);

            try
            {
                IsLoading = true;

                var strategies = await strategyService.GetStrategies().ConfigureAwait(true);

                Strategies.Clear();
                strategies.ForEach(s => Strategies.Add(s));
            }
            catch (Exception ex)
            {
                ShowMessage(new Message { MessageType = MessageType.Error, Text = ex.Message });
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        protected override void OnDisposing()
        {
            if(disposed)
            {
                return;           
            }

            foreach (var subscription in strategyObservableSubscriptions.Values)
            {
                subscription.Dispose();
            }

            foreach (var strategyViewModel in SelectedStrategyViewModels)
            {
                strategyViewModel.Dispose();
            }

            disposed = true;
        }

        protected async override void SaveDocument()
        {
            try
            {
                IsLoading = true;

                foreach (var strategyViewModel in SelectedStrategyViewModels)
                {
                    await strategyService.SaveStrategy(strategyViewModel.Strategy).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(new Message { MessageType = MessageType.Error, Text = ex.Message });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AddStrategy(object param)
        {
            if (param == null
                || string.IsNullOrEmpty(param.ToString()))
            {
                return;
            }

            var strategyName = param.ToString();

            if (Strategies.Any(s => s.Name.Equals(strategyName, StringComparison.Ordinal)))
            {
                ShowMessage(new Message { MessageType = MessageType.Info, Text = $"A strategy with the name {strategyName} already exists." });
                return;
            }

            try
            {
                IsLoading = true;

                var strategy = new Strategy { Name = strategyName };
                await strategyService.SaveStrategy(strategy).ConfigureAwait(true);
                Strategies.Add(strategy);
                ConfigurationModule.AddStrategy(strategy.Name);
            }
            catch (Exception ex)
            {
                ShowMessage(new Message { MessageType = MessageType.Error, Text = ex.Message });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void DeleteStrategy(object param)
        {
            if (param is Strategy strategy)
            {
                var result = Dialog.ShowMessage(new MessageBoxSettings
                {
                    Title = "Delete Strategy",
                    Text = $"Are you sure you want to delete {strategy.Name}?",
                    MessageType = MessageType.Question,
                    MessageBoxButtons = MessageBoxButtons.OkCancel
                });

                if (result.Equals(MessageBoxResult.Cancel))
                {
                    return;
                }

                var strategyViewModel = SelectedStrategyViewModels.FirstOrDefault(s => s.Strategy.Name.Equals(strategy.Name, StringComparison.Ordinal));
                if (strategyViewModel != null)
                {
                    Close(strategyViewModel);
                }

                try
                {
                    IsLoading = true;

                    await strategyService.DeleteStrategy(strategy).ConfigureAwait(true);
                    Strategies.Remove(strategy);
                    ConfigurationModule.RemoveStrategy(strategy.Name);
                }
                catch (Exception ex)
                {
                    ShowMessage(new Message { MessageType = MessageType.Error, Text = ex.Message });
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void ObserveStrategy(StrategyViewModel strategy)
        {
            var strateyObservable = Observable.FromEventPattern<StrategyEventArgs>(
                eventHandler => strategy.OnStrategyNotification += eventHandler,
                eventHandler => strategy.OnStrategyNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            var strateyObservableSubscription = strateyObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    ShowMessage(new Message { MessageType = MessageType.Error, Text = args.Exception.ToString() });
                }
                else
                {
                    ShowMessage(new Message { MessageType = MessageType.Info, Text = args.Message });
                }
            });

            strategyObservableSubscriptions.Add(strategy.Strategy.Name, strateyObservableSubscription);
        }
    }
}