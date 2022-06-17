﻿using DevelopmentInProgress.TradeView.Core.Enums;
using DevelopmentInProgress.TradeView.Core.Extensions;
using DevelopmentInProgress.TradeView.Core.Validation;
using DevelopmentInProgress.TradeView.Test.Helper;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using DevelopmentInProgress.TradeView.Wpf.Common.Services;
using DevelopmentInProgress.TradeView.Wpf.Trading.Events;
using DevelopmentInProgress.TradeView.Wpf.Trading.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using InterfaceExtensions = DevelopmentInProgress.TradeView.Core.Extensions;

namespace DevelopmentInProgress.TradeView.Wpf.Trading.Test
{
    [TestClass]
    public class TradePanelViewModelTest
    {
        [TestMethod]
        public async Task SetAccount()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);

            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SetSymbols(symbols.ToList());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            
            // Act
            tradeViewModel.SetAccount(account);

            // Assert
            Assert.AreEqual(tradeViewModel.Account, account);
            Assert.AreEqual(tradeViewModel.SelectedOrderType, null);
            Assert.IsNull(tradeViewModel.SelectedSymbol);
        }

        [TestMethod]
        public async Task SetSymbol()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);

            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SetSymbols(symbols.ToList());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var asset = account.Balances.Single(a => a.Asset.Equals("TRX"));
            tradeViewModel.SetAccount(account);
            
            // Act
            tradeViewModel.SetSymbol(asset);

            // Assert
            Assert.AreEqual(tradeViewModel.Account, account);
            Assert.AreEqual(tradeViewModel.SelectedOrderType, null);
            Assert.AreEqual(tradeViewModel.SelectedSymbol.Name, trx.Name);
        }

        [TestMethod]
        public async Task SetAccount_Different_Account_Null_SelectedAsset()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);

            tradeViewModel.SetSymbols(symbols.ToList());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);

            tradeViewModel.SetAccount(account);

            var differentAccount = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "test123",
                ApiSecret = "test123"
            };

            // Act
            tradeViewModel.SetAccount(differentAccount);

            // Assert
            Assert.AreEqual(tradeViewModel.Account, differentAccount);
            Assert.AreEqual(tradeViewModel.SelectedOrderType, null);
            Assert.IsNull(tradeViewModel.SelectedSymbol);
        }

        [TestMethod]
        public async Task SelectedSymbol_NoAccount()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));
            
            // Act
            tradeViewModel.SelectedSymbol = trx;

            // Assert
            Assert.AreEqual(tradeViewModel.SelectedSymbol, trx);
            Assert.AreEqual(tradeViewModel.Quantity, 0);
            Assert.AreEqual(tradeViewModel.Price, trx.SymbolStatistics.LastPrice);
            Assert.AreEqual(tradeViewModel.StopPrice, trx.SymbolStatistics.LastPrice);
            Assert.IsNull(tradeViewModel.BaseAccountBalance);
            Assert.IsNull(tradeViewModel.QuoteAccountBalance);
        }

        [TestMethod]
        public async Task SelectedSymbol_HasAccount()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var baseBalance = account.Balances.Single(a => a.Asset.Equals("TRX"));
            var quoteAsset = account.Balances.Single(a => a.Asset.Equals("BTC"));

            tradeViewModel.SetAccount(account);

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            // Act
            tradeViewModel.SelectedSymbol = trx;

            // Assert
            Assert.AreEqual(tradeViewModel.SelectedSymbol, trx);
            Assert.AreEqual(tradeViewModel.Quantity, 0);
            Assert.AreEqual(tradeViewModel.Price, trx.SymbolStatistics.LastPrice);
            Assert.AreEqual(tradeViewModel.StopPrice, trx.SymbolStatistics.LastPrice);
            Assert.AreEqual(tradeViewModel.BaseAccountBalance, baseBalance);
            Assert.AreEqual(tradeViewModel.QuoteAccountBalance, quoteAsset);
        }

        [TestMethod]
        public async Task SelectedSymbol_Null()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);

            tradeViewModel.SetAccount(account);

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SelectedSymbol = trx;

            // Act
            tradeViewModel.SelectedSymbol = null;

            // Assert
            Assert.IsNull(tradeViewModel.SelectedSymbol);
            Assert.AreEqual(tradeViewModel.Quantity, 0);
            Assert.AreEqual(tradeViewModel.Price, 0);
            Assert.AreEqual(tradeViewModel.StopPrice, 0);
            Assert.IsNull(tradeViewModel.BaseAccountBalance);
            Assert.IsNull(tradeViewModel.QuoteAccountBalance);
        }

        [TestMethod]
        public async Task Quantity_and_Price_and_StopPrice_Trim()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SelectedSymbol = trx;

            var quantity = 294.123m;
            var price = 1.123456789m;

            // Act
            tradeViewModel.Quantity = quantity;
            tradeViewModel.Price = price;
            tradeViewModel.StopPrice = price;

            // Assert
            Assert.AreEqual(tradeViewModel.Quantity, quantity.Trim(trx.QuantityPrecision));
            Assert.AreEqual(tradeViewModel.Price, price.Trim(trx.PricePrecision));
            Assert.AreEqual(tradeViewModel.StopPrice, price.Trim(trx.PricePrecision));
        }

        [TestMethod]
        public async Task Quantity_and_Price_and_StopPrice_NoTrim()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SelectedSymbol = trx;

            var quantity = 294m;
            var price = 1.12345678m;

            // Act
            tradeViewModel.Quantity = quantity;
            tradeViewModel.Price = price;
            tradeViewModel.StopPrice = price;

            // Assert
            Assert.AreEqual(tradeViewModel.Quantity, quantity.Trim(trx.QuantityPrecision));
            Assert.AreEqual(tradeViewModel.Price, price.Trim(trx.PricePrecision));
            Assert.AreEqual(tradeViewModel.StopPrice, price.Trim(trx.PricePrecision));
        }


        [TestMethod]
        public void Quantity_and_Price_and_StopPrice_NoSelectedSymbol()
        {
            // Arrange
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var quantity = 294.123m;
            var price = 1.123456789m;

            // Act
            tradeViewModel.Quantity = quantity;
            tradeViewModel.Price = price;
            tradeViewModel.StopPrice = price;

            // Assert
            Assert.AreEqual(tradeViewModel.Quantity, quantity);
            Assert.AreEqual(tradeViewModel.Price, price);
            Assert.AreEqual(tradeViewModel.StopPrice, price);
        }
        
        [TestMethod]
        public async Task SelectedOrderType_SelectedSymbol()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SelectedSymbol = trx;

            // Act
            tradeViewModel.SelectedOrderType = "Limit";

            // Assert
            Assert.AreEqual(tradeViewModel.SelectedOrderType, "Limit");
            Assert.AreEqual(tradeViewModel.Price, trx.SymbolStatistics.LastPrice);
            Assert.AreEqual(tradeViewModel.StopPrice, trx.SymbolStatistics.LastPrice);
        }

        [TestMethod]
        public async Task SelectedOrderType_IsMarketOrder_Not_IsLoading()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SelectedSymbol = trx;

            // Act
            tradeViewModel.SelectedOrderType = "Market";

            // Assert
            Assert.IsFalse(tradeViewModel.IsPriceEditable);
            Assert.IsTrue(tradeViewModel.IsMarketPrice);
        }

        [TestMethod]
        public async Task SelectedOrderType_IsMarketOrder_IsLoading()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.IsLoading = true;
            tradeViewModel.SelectedSymbol = trx;

            // Act
            tradeViewModel.SelectedOrderType = "Market";

            // Assert
            Assert.IsFalse(tradeViewModel.IsPriceEditable);
            Assert.IsFalse(tradeViewModel.IsMarketPrice);
        }

        [TestMethod]
        public async Task SelectedOrderType_IsStopLoss_Not_IsLoading()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.SelectedSymbol = trx;

            // Act
            tradeViewModel.SelectedOrderType = "Stop Loss";

            // Assert
            Assert.IsFalse(tradeViewModel.IsPriceEditable);
            Assert.IsTrue(tradeViewModel.IsMarketPrice);
        }

        [TestMethod]
        public async Task SelectedOrderType_IsStopLoss_IsLoading()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            tradeViewModel.SetSymbols(symbols.ToList());
            var trx = tradeViewModel.Symbols.Single(s => s.Name.Equals("TRXBTC"));

            tradeViewModel.IsLoading = true;
            tradeViewModel.SelectedSymbol = trx;

            // Act
            tradeViewModel.SelectedOrderType = "Stop Loss";

            // Assert
            Assert.IsFalse(tradeViewModel.IsPriceEditable);
            Assert.IsFalse(tradeViewModel.IsMarketPrice);
        }

        [TestMethod]
        public void HasQuoteBaseBalance_Null()
        {
            // Arrange
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());
            
            // Act

            // Assert
            Assert.IsFalse(tradeViewModel.HasBaseBalance);
            Assert.IsFalse(tradeViewModel.HasQuoteBalance);
        }

        [TestMethod]
        public void HasQuoteBaseBalance_Zero()
        {
            // Arrange
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var quoteBalance = new AccountBalance { Free = 0m };
            var baseBalance = new AccountBalance { Free = 0m };

            // Act
            tradeViewModel.QuoteAccountBalance = quoteBalance;
            tradeViewModel.BaseAccountBalance = baseBalance;

            // Assert
            Assert.IsFalse(tradeViewModel.HasBaseBalance);
            Assert.IsFalse(tradeViewModel.HasQuoteBalance);
        }

        [TestMethod]
        public void HasQuoteBaseBalance()
        {
            // Arrange
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var quoteBalance = new AccountBalance { Free = 299m };
            var baseBalance = new AccountBalance { Free = 0.000123m };

            // Act
            tradeViewModel.QuoteAccountBalance = quoteBalance;
            tradeViewModel.BaseAccountBalance = baseBalance;

            // Assert
            Assert.IsTrue(tradeViewModel.HasBaseBalance);
            Assert.IsTrue(tradeViewModel.HasQuoteBalance);
        }

        [TestMethod]
        public async Task OrderTypes_NoSelectedSymbol()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);

            tradeViewModel.SetSymbols(symbols.ToList());

            // Act
            
            // Assert
            Assert.IsNull(tradeViewModel.SelectedSymbol);

            var intersection = tradeViewModel.OrderTypes.Intersect(InterfaceExtensions.OrderExtensions.OrderTypes()).ToList();
            Assert.IsTrue(InterfaceExtensions.OrderExtensions.OrderTypes().Count().Equals(intersection.Count));
        }

        [TestMethod]
        public async Task OrderTypes_SelectedSymbol()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);

            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));
            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SelectedSymbol = trx;

            // Act
            var orderTypes = tradeViewModel.OrderTypes;

            // Assert
            Assert.AreEqual(tradeViewModel.SelectedSymbol, trx);
            
            var missing = OrderExtensions.OrderTypes().Except(tradeViewModel.OrderTypes).ToList();
            foreach(var orderType in missing)
            {
                if (orderType != OrderExtensions.GetOrderTypeName(Core.Model.OrderType.StopLoss)
                    && orderType != OrderExtensions.GetOrderTypeName(Core.Model.OrderType.TakeProfit))
                {
                    Assert.Fail();
                }
            }
        }
        
        [TestMethod]
        public async Task BuyQuantity_InsufficientFunds()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);

            // Act
            tradeViewModel.BuyQuantityCommand.Execute(75);

            // Assert
            Assert.IsTrue(tradeViewModel.Quantity.Equals(0));
        }

        [TestMethod]
        public async Task BuyQuantity_SufficientFunds()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);
            tradeViewModel.QuoteAccountBalance.Free = 0.00012693M;

            // Act
            tradeViewModel.BuyQuantityCommand.Execute(75);

            // Assert
            Assert.IsTrue(tradeViewModel.Quantity.Equals(10));
        }

        [TestMethod]
        public async Task SellQuantity()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);

            // Act
            tradeViewModel.SellQuantityCommand.Execute(75);

            // Assert
            Assert.IsTrue(tradeViewModel.Quantity.Equals((selectedAsset.Free*0.75m).Trim(trx.QuantityPrecision)));
        }

        [TestMethod]
        public async Task Buy_Pass()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);
            
            tradeViewModel.SelectedOrderType = "Limit";
            tradeViewModel.Quantity = 200m;
            tradeViewModel.Price = 0.00000900M;
            tradeViewModel.QuoteAccountBalance.Free = 200m * 0.00000900M;

            var tradeObservable = Observable.FromEventPattern<TradeEventArgs>(
                eventHandler => tradeViewModel.OnTradeNotification += eventHandler,
                eventHandler => tradeViewModel.OnTradeNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            Exception ex = null;
            tradeObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    ex = args.Exception;
                }
            });

            // Act
            tradeViewModel.BuyCommand.Execute(null);

            // Assert
            Assert.IsNull(ex);
        }

        [TestMethod]
        public async Task Buy_Fails_No_OrderType()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);
            
            tradeViewModel.Quantity = 200m;
            tradeViewModel.Price = 0.00000900M;

            var tradeObservable = Observable.FromEventPattern<TradeEventArgs>(
                eventHandler => tradeViewModel.OnTradeNotification += eventHandler,
                eventHandler => tradeViewModel.OnTradeNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            Exception ex = null;
            tradeObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    ex = args.Exception;
                }
            });

            // Act
            tradeViewModel.BuyCommand.Execute(null);

            // Assert
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains("orderType"));
        }

        [TestMethod]
        public async Task Buy_Fails_Order_Validation()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);

            tradeViewModel.QuoteAccountBalance.Free = 0.00012693M;

            tradeViewModel.SelectedOrderType = "Limit";
            tradeViewModel.Price = 0.00000900M;

            var tradeObservable = Observable.FromEventPattern<TradeEventArgs>(
                eventHandler => tradeViewModel.OnTradeNotification += eventHandler,
                eventHandler => tradeViewModel.OnTradeNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            Exception ex = null;
            tradeObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    ex = args.Exception;
                }
            });

            // Act
            tradeViewModel.BuyCommand.Execute(null);

            // Assert
            Assert.IsInstanceOfType(ex, typeof(OrderValidationException));
        }

        [TestMethod]
        public async Task Buy_Fails_PlaceOrder()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService(ExchangeServiceType.PlaceOrderException);
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);

            tradeViewModel.QuoteAccountBalance.Free = 0.00012693M;

            tradeViewModel.SelectedOrderType = "Limit";
            tradeViewModel.Quantity = 200m;
            tradeViewModel.Price = 0.00000900M;
            tradeViewModel.QuoteAccountBalance.Free = 200m * 0.00000900M;

            var tradeObservable = Observable.FromEventPattern<TradeEventArgs>(
                eventHandler => tradeViewModel.OnTradeNotification += eventHandler,
                eventHandler => tradeViewModel.OnTradeNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            Exception ex = null;
            tradeObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    ex = args.Exception;
                }
            });

            // Act
            tradeViewModel.BuyCommand.Execute(null);

            // Assert
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains("failed to place order"));
        }

        [TestMethod]
        public async Task Sell_Pass()
        {
            // Arrange
            var cxlToken = new CancellationToken();
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var tradeViewModel = new TradePanelViewModel(exchangeService, new DebugLogger());

            var symbols = await exchangeService.GetSymbols24HourStatisticsAsync(Exchange.Test, cxlToken);
            var trx = symbols.Single(s => s.Name.Equals("TRXBTC"));

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            account = await exchangeService.GetAccountInfoAsync(Exchange.Test, account.AccountInfo.User, cxlToken);
            var selectedAsset = account.Balances.Single(ab => ab.Asset.Equals("TRX"));

            tradeViewModel.SetSymbols(symbols.ToList());
            tradeViewModel.SetAccount(account);
            tradeViewModel.SetSymbol(selectedAsset);

            tradeViewModel.QuoteAccountBalance.Free = 0.00012693M;

            tradeViewModel.SelectedOrderType = "Limit";
            tradeViewModel.Quantity = tradeViewModel.BaseAccountBalance.Free;
            tradeViewModel.Price = 0.00000850M;

            var tradeObservable = Observable.FromEventPattern<TradeEventArgs>(
                eventHandler => tradeViewModel.OnTradeNotification += eventHandler,
                eventHandler => tradeViewModel.OnTradeNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            Exception ex = null;
            tradeObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    ex = args.Exception;
                }
            });

            // Act
            tradeViewModel.SellCommand.Execute(null);

            // Assert
            Assert.IsNull(ex);
        }
    }
}