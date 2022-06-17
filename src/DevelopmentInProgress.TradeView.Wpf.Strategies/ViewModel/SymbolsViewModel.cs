﻿using DevelopmentInProgress.TradeView.Wpf.Common.Cache;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using DevelopmentInProgress.TradeView.Wpf.Common.ViewModel;
using DevelopmentInProgress.TradeView.Wpf.Strategies.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.TradeView.Wpf.Strategies.ViewModel
{
    public class SymbolsViewModel : BaseViewModel
    {
        private readonly ISymbolsCacheFactory symbolsCacheFactory;
        private ISymbolsCache symbolsCache;
        private Strategy strategy;
        private bool isLoadingSymbols;
        private bool disposed;

        public SymbolsViewModel(ISymbolsCacheFactory symbolsCacheFactory, ILoggerFacade logger)
            : base(logger)
        {
            this.symbolsCacheFactory = symbolsCacheFactory;

            Symbols = new ObservableCollection<Symbol>();
        }

        public event EventHandler<StrategySymbolsEventArgs> OnSymbolsNotification;

        public ObservableCollection<Symbol> Symbols { get; }

        public Strategy Strategy
        {
            get { return strategy; }
            set
            {
                if (strategy != value)
                {
                    strategy = value;
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
                if(symbolsCache != null)
                {
                    symbolsCache.OnSymbolsCacheException -= SymbolsCacheException;
                }
            }

            disposed = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Notify subscribers of exception.")]
        public async Task GetStrategySymbols(Strategy arg)
        {
            IsLoadingSymbols = true;

            try
            {
                Strategy = arg;

                if(symbolsCache == null)
                {
                    symbolsCache = symbolsCacheFactory.GetSymbolsCache(Strategy.StrategySubscriptions.First().Exchange);

                    symbolsCache.OnSymbolsCacheException += SymbolsCacheException;
                }

                var strategySymbols = Strategy.StrategySubscriptions.Select(s => s.Symbol);

                var results = await symbolsCache.GetSymbols(strategySymbols).ConfigureAwait(true);

                var symbols = results.Where(r => strategySymbols.Contains($"{r.ExchangeSymbol}")).ToList();

                Symbols.Clear();

                symbols.ForEach(Symbols.Add);

                SymbolsNotification();
            }
            catch (Exception ex)
            {
                OnException($"SymbolsViewModel.GetSymbols {ex.Message}", ex);
            }

            IsLoadingSymbols = false;
        }

        private void SymbolsCacheException(object sender, Exception exception)
        {
            OnException($"SymbolsCache.GetSymbols {exception.Message}", exception);
        }

        private void OnException(string message, Exception exception)
        {
            var onSymbolsNotification = OnSymbolsNotification;
            onSymbolsNotification?.Invoke(this, new StrategySymbolsEventArgs { Message = message, Exception = exception });
        }

        private void SymbolsNotification()
        {
            var onSymbolsNotification = OnSymbolsNotification;
            onSymbolsNotification?.Invoke(this, new StrategySymbolsEventArgs { Value = Symbols.ToList() });
        }
    }
}
