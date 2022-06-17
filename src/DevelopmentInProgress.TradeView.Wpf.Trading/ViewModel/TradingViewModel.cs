﻿using DevelopmentInProgress.TradeView.Wpf.Common.Chart;
using DevelopmentInProgress.TradeView.Wpf.Common.Events;
using DevelopmentInProgress.TradeView.Wpf.Common.Helpers;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using DevelopmentInProgress.TradeView.Wpf.Common.Services;
using DevelopmentInProgress.TradeView.Wpf.Common.ViewModel;
using DevelopmentInProgress.TradeView.Wpf.Controls.Messaging;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.Context;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.ViewModel;
using DevelopmentInProgress.TradeView.Wpf.Trading.Events;
using Newtonsoft.Json;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.TradeView.Wpf.Trading.ViewModel
{
    public class TradingViewModel : DocumentViewModel
    {
        private readonly IOrderBookHelperFactory orderBookHelperFactory;
        private readonly ITradeHelperFactory tradeHelperFactory;
        private readonly IWpfExchangeService exchangeService;
        private readonly IAccountsService accountsService;
        private readonly IChartHelper chartHelper;
        private AccountViewModel accountViewModel;
        private SymbolViewModel symbolViewModel;
        private TradePanelViewModel tradePanelViewModel;
        private SymbolsViewModel symbolsViewModel;
        private UserAccount userAccount;
        private Account account;
        private Symbol symbol;
        private bool isOpen;
        private bool disposed;

        private IDisposable symbolsObservableSubscription;
        private IDisposable symbolObservableSubscription;
        private IDisposable accountObservableSubscription;
        private IDisposable tradeObservableSubscription;

        public TradingViewModel(ViewModelContext viewModelContext, 
            AccountViewModel accountViewModel, 
            SymbolsViewModel symbolsViewModel,
            TradePanelViewModel tradePanelViewModel,
            IWpfExchangeService exchangeService, 
            IAccountsService accountsService,
            IOrderBookHelperFactory orderBookHelperFactory,
            ITradeHelperFactory tradeHelperFactory,
            IChartHelper chartHelper)
            : base(viewModelContext)
        {
            AccountViewModel = accountViewModel;
            SymbolsViewModel = symbolsViewModel;
            TradeViewModel = tradePanelViewModel;

            this.exchangeService = exchangeService;
            this.accountsService = accountsService;
            this.orderBookHelperFactory = orderBookHelperFactory;
            this.tradeHelperFactory = tradeHelperFactory;
            this.chartHelper = chartHelper;

            ObserveSymbols();
            ObserveAccount();
            ObserveTrade();
        }

        public AccountViewModel AccountViewModel
        {
            get { return accountViewModel; }
            private set
            {
                if (accountViewModel != value)
                {
                    accountViewModel = value;
                    OnPropertyChanged(nameof(AccountViewModel));
                }
            }
        }

        public SymbolsViewModel SymbolsViewModel
        {
            get { return symbolsViewModel; }
            private set
            {
                if (symbolsViewModel != value)
                {
                    symbolsViewModel = value;
                    OnPropertyChanged(nameof(SymbolsViewModel));
                }
            }
        }

        public TradePanelViewModel TradeViewModel
        {
            get { return tradePanelViewModel; }
            private set
            {
                if (tradePanelViewModel != value)
                {
                    tradePanelViewModel = value;
                    OnPropertyChanged(nameof(TradeViewModel));
                }
            }
        }

        public SymbolViewModel SymbolViewModel
        {
            get { return symbolViewModel; }
            private set
            {
                if (symbolViewModel != value)
                {
                    symbolViewModel = value;
                    OnPropertyChanged(nameof(SymbolViewModel));
                }
            }
        }

        public Account Account
        {
            get { return account; }
            private set
            {
                if (account != value)
                {
                    account = value;
                    OnPropertyChanged(nameof(Account));
                }
            }
        }

        protected async override void OnPublished(object data)
        {
            if(isOpen)
            {
                return;
            }

            IsBusy = true;

            if(Messages != null
                && Messages.Any())
            {
                ClearMessages();
            }

            accountViewModel.Dispatcher = ViewModelContext.UiDispatcher;
            symbolsViewModel.Dispatcher = ViewModelContext.UiDispatcher;
            tradePanelViewModel.Dispatcher = ViewModelContext.UiDispatcher;

            Account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() });

            userAccount = await accountsService.GetAccountAsync(Title).ConfigureAwait(true);
            var json = JsonConvert.SerializeObject(userAccount, Formatting.Indented);
            Logger.Log(json, Category.Info, Priority.Medium);

            if (userAccount != null
                && userAccount.Preferences != null)
            {
                if (!string.IsNullOrWhiteSpace(userAccount.ApiKey))
                {
                    Account.AccountName = userAccount.AccountName;
                    Account.ApiKey = userAccount.ApiKey;
                    Account.ApiSecret = userAccount.ApiSecret;
                    Account.ApiPassPhrase = userAccount.ApiPassPhrase;
                    Account.Exchange = userAccount.Exchange;
                }
            }

            await Task.WhenAll(SymbolsViewModel.SetAccount(userAccount), AccountViewModel.Login(Account)).ConfigureAwait(true);

            isOpen = true;
            IsBusy = false;
        }

        protected override void OnDisposing()
        {
            if (disposed)
            {
                return;
            }

            symbolsObservableSubscription?.Dispose();
            tradeObservableSubscription?.Dispose();
            accountObservableSubscription.Dispose();
            symbolObservableSubscription?.Dispose();

            AccountViewModel.Dispose();
            SymbolsViewModel?.Dispose();
            TradeViewModel?.Dispose();
            SymbolViewModel?.Dispose();

            disposed = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Feed all exceptions back to subscribers")]
        public async override void OnActiveChanged(bool isActive)
        {
            try
            {
                if (isActive)
                {
                    var openDocuments = new FindDocumentViewModel { Module = "Trading" };

                    OnGetViewModels(openDocuments);

                    var tradingViewModels = openDocuments.ViewModels.OfType<TradingViewModel>()
                        .Where(d => d.SymbolViewModel != null && d.SymbolViewModel.IsActive).ToList();

                    foreach (var tradingViewModel in tradingViewModels)
                    {
                        tradingViewModel.DisposeSymbolViewModel();
                    }

                    await LoadSymbolViewModel().ConfigureAwait(false);
                }
                else
                {
                    DisposeSymbolViewModel();
                }
            }
            catch (Exception ex)
            {
                TradingViewModelException(ex.ToString(), ex);
            }
        }

        private void DisposeSymbolViewModel()
        {
            if (SymbolViewModel != null)
            {
                SymbolViewModel.Dispose();
                SymbolViewModel = null;
            }

            if (symbolObservableSubscription != null)
            {
                symbolObservableSubscription.Dispose();
                symbolObservableSubscription = null;
            }
        }

        private async Task LoadSymbolViewModel()
        {
            if (symbol == null)
            {
                return;
            }

            SymbolViewModel = new SymbolViewModel(
                userAccount.Exchange, exchangeService, chartHelper,
                orderBookHelperFactory.GetOrderBookHelper(userAccount.Exchange),
                tradeHelperFactory.GetTradeHelper(userAccount.Exchange),
                userAccount.Preferences, Logger)
            {
                Dispatcher = ViewModelContext.UiDispatcher
            };

            ObserveSymbol(SymbolViewModel);

            await SymbolViewModel.SetSymbol(symbol).ConfigureAwait(true);

            SymbolViewModel.IsActive = true;
        }

        private void ObserveSymbols()
        {
            var symbolsObservable = Observable.FromEventPattern<SymbolsEventArgs>(
                eventHandler => SymbolsViewModel.OnSymbolsNotification += eventHandler,
                eventHandler => SymbolsViewModel.OnSymbolsNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            symbolsObservableSubscription = symbolsObservable.Subscribe(async args =>
            {
                if (args.HasException)
                {
                    TradingViewModelException(args);
                }
                else if (args.Value != null)
                {
                    symbol = args.Value;
                    await LoadSymbolViewModel().ConfigureAwait(false);
                }
                else if (args.Symbols.Any())
                {
                    TradeViewModel.SetSymbols(args.Symbols);
                }
            });
        }

        private void ObserveAccount()
        {
            var accountObservable = Observable.FromEventPattern<AccountEventArgs>(
                eventHandler => AccountViewModel.OnAccountNotification += eventHandler,
                eventHandler => AccountViewModel.OnAccountNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            accountObservableSubscription = accountObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    TradingViewModelException(args);
                }
                else if (args.AccountEventType.Equals(AccountEventType.LoggedIn))
                {
                    TradeViewModel.SetAccount(args.Value);
                }
                else if (args.AccountEventType.Equals(AccountEventType.UpdateOrders))
                {
                    TradeViewModel.Touch();
                }
                else if (args.AccountEventType.Equals(AccountEventType.OrdersNotification))
                {
                    TradingViewModelException(args);
                }
                else if (args.AccountEventType.Equals(AccountEventType.SelectedAsset))
                {
                    TradeViewModel.SetSymbol(args.SelectedAsset);
                }
            });
        }

        private void ObserveSymbol(SymbolViewModel symbol)
        {
            var symbolObservable = Observable.FromEventPattern<SymbolEventArgs>(
                eventHandler => symbol.OnSymbolNotification += eventHandler,
                eventHandler => symbol.OnSymbolNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            symbolObservableSubscription = symbolObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    TradingViewModelException(args);
                }
            });
        }

        private void ObserveTrade()
        {
            var tradeObservable = Observable.FromEventPattern<TradeEventArgs>(
                eventHandler => TradeViewModel.OnTradeNotification += eventHandler,
                eventHandler => TradeViewModel.OnTradeNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            tradeObservableSubscription = tradeObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    TradingViewModelException(args);
                }
            });
        }

        private void TradingViewModelException<T>(BaseEventArgs<T> eventArgs)
        {
            if (eventArgs.Exception != null)
            {
                TradingViewModelException(eventArgs.Message, eventArgs.Exception);
            }
            else
            {
                Logger.Log(eventArgs.Message, Category.Exception, Priority.High);
            }
        }

        private void TradingViewModelException(string message, Exception ex)
        {
            Logger.Log(message, Category.Exception, Priority.High);

            var exceptions = new List<Message>();
            if (ex is AggregateException aex)
            {
                foreach(Exception e in aex.InnerExceptions)
                {
                    Logger.Log(e.ToString(), Category.Exception, Priority.High);
                    exceptions.Add(new Message { MessageType = MessageType.Error, Text = e.Message, TextVerbose = e.StackTrace });
                }
            }
            else
            {
                Logger.Log(ex.ToString(), Category.Exception, Priority.High);
                exceptions.Add(new Message { MessageType = MessageType.Error, Text = ex.Message, TextVerbose = ex.StackTrace });
            }

            ShowMessages(exceptions);
        }
    }
}