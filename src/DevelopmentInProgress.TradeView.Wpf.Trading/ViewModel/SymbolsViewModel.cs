﻿using DevelopmentInProgress.TradeView.Wpf.Common.Cache;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using DevelopmentInProgress.TradeView.Wpf.Common.Services;
using DevelopmentInProgress.TradeView.Wpf.Common.ViewModel;
using DevelopmentInProgress.TradeView.Wpf.Trading.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.TradeView.Wpf.Trading.ViewModel
{
    public class SymbolsViewModel : ExchangeViewModel
    {
        private readonly ISymbolsCacheFactory symbolsCacheFactory;
        private ISymbolsCache symbolsCache;
        private Symbol selectedSymbol;
        private UserAccount accountPreferences;
        private bool isLoadingSymbols;
        private bool disposed;

        public SymbolsViewModel(IWpfExchangeService exchangeService, ISymbolsCacheFactory symbolsCacheFactory, ILoggerFacade logger)
            : base(exchangeService, logger)
        {
            this.symbolsCacheFactory = symbolsCacheFactory;

            Symbols = new ObservableCollection<Symbol>();
        }

        public event EventHandler<SymbolsEventArgs> OnSymbolsNotification;

        public ObservableCollection<Symbol> Symbols { get; }

        public Symbol SelectedSymbol
        {
            get { return selectedSymbol; }
            set
            {
                if (selectedSymbol != value)
                {
                    selectedSymbol = value;
                    OnSelectedSymbol(selectedSymbol);
                    OnPropertyChanged(nameof(SelectedSymbol));
                }
            }
        }

        public UserAccount AccountPreferences
        {
            get { return accountPreferences; }
            set
            {
                if (accountPreferences != value)
                {
                    accountPreferences = value;
                }
            }
        }

        public bool IsLoadingSymbols
        {
            get { return isLoadingSymbols; }
            set
            {
                if (isLoadingSymbols != value)
                {
                    isLoadingSymbols = value;
                    OnPropertyChanged(nameof(IsLoadingSymbols));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (symbolsCache != null)
                {
                    symbolsCache.OnSymbolsCacheException -= SymbolsCacheException;
                }
            }

            disposed = true;
        }

        public async Task SetAccount(UserAccount userAccount)
        {
            IsLoadingSymbols = true;

            AccountPreferences = userAccount;

            await GetSymbols().ConfigureAwait(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Feed all exceptions back to subscribers")]
        private async Task GetSymbols()
        {
            try
            {
                if (symbolsCache == null)
                {
                    symbolsCache = symbolsCacheFactory.GetSymbolsCache(AccountPreferences.Exchange);
                    symbolsCache.OnSymbolsCacheException += SymbolsCacheException;
                }

                var results = await symbolsCache.GetSymbols(AccountPreferences.Preferences.FavouriteSymbols).ConfigureAwait(true);

                results.ForEach(Symbols.Add);

                OnLoadedSymbols(Symbols.ToList());

                SetPreferences();
            }
            catch (Exception ex)
            {
                OnException($"{nameof(SymbolsViewModel)} - {ex.Message}", ex);
            }
            finally
            {
                IsLoadingSymbols = false;
            }
        }

        private void SymbolsCacheException(object sender, Exception exception)
        {
            OnException($"{nameof(SymbolsViewModel)} - {exception.Message}", exception);
        }

        private void OnException(string message, Exception exception)
        {
            var onSymbolsNotification = OnSymbolsNotification;
            onSymbolsNotification?.Invoke(this, new SymbolsEventArgs() { Message = message, Exception = exception });
        }

        private void OnSelectedSymbol(Symbol symbol)
        {
            var onSymbolsNotification = OnSymbolsNotification;
            onSymbolsNotification?.Invoke(this, new SymbolsEventArgs { Value = symbol });
        }

        private void OnLoadedSymbols(List<Symbol> symbols)
        {
            var onSymbolsNotification = OnSymbolsNotification;
            var symbolsEventArgs = new SymbolsEventArgs(symbols);
            onSymbolsNotification?.Invoke(this, symbolsEventArgs);
        }

        private void SetPreferences()
        {
            if (AccountPreferences != null 
                && AccountPreferences.Preferences != null 
                && Symbols.Any())
            {
                if (AccountPreferences.Preferences.FavouriteSymbols != null
                    && AccountPreferences.Preferences.FavouriteSymbols.Any())
                {
                    if (!string.IsNullOrWhiteSpace(AccountPreferences.Preferences.SelectedSymbol))
                    {
                        var symbol = Symbols.FirstOrDefault(s => s.ExchangeSymbol.Equals(AccountPreferences.Preferences.SelectedSymbol, StringComparison.OrdinalIgnoreCase));
                        if (symbol != null)
                        {
                            SelectedSymbol = symbol;
                        }
                    }
                }
            }
        }
    }
}
