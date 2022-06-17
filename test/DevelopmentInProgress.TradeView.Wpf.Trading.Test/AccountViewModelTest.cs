﻿using DevelopmentInProgress.TradeView.Test.Helper;
using DevelopmentInProgress.TradeView.Wpf.Common.Events;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using DevelopmentInProgress.TradeView.Wpf.Common.Services;
using DevelopmentInProgress.TradeView.Wpf.Common.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DevelopmentInProgress.TradeView.Wpf.Trading.Test
{
    [TestClass]
    public class AccountViewModelTest
    {
        [TestMethod]
        public async Task Login()
        {
            // Arrange
            var fail = false;
            Account notifyAccount = null;
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolsCacheFactory = SymbolsCacheFactoryHelper.GetSymbolsCachefactory(exchangeService);
            var accountViewModel = new AccountBalancesViewModel(exchangeService, symbolsCacheFactory, new DebugLogger());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            var accountObservable = Observable.FromEventPattern<AccountEventArgs>(
                eventHandler => accountViewModel.OnAccountNotification += eventHandler,
                eventHandler => accountViewModel.OnAccountNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            accountObservable.Subscribe(args =>
            {
                if (args.AccountEventType.Equals(AccountEventType.LoggedIn))
                {
                    notifyAccount = args.Value;
                }
                else
                {
                    fail = true;
                }
            });

            // Act
            await accountViewModel.Login(account);

            // Assert
            Assert.AreEqual(account.ApiKey, accountViewModel.Account.ApiKey);
            Assert.AreEqual(account.AccountInfo.User.ApiSecret, accountViewModel.Account.AccountInfo.User.ApiSecret);
            Assert.IsNotNull(notifyAccount);
            Assert.AreEqual(account.ApiKey, notifyAccount.ApiKey);
            Assert.AreEqual(account.AccountInfo.User.ApiSecret, notifyAccount.AccountInfo.User.ApiSecret);
            Assert.IsFalse(fail);
        }

        [TestMethod]
        public async Task Login_AccountInfo_Update()
        {
            // Arrange
            var fail = false;
            var hasUpdated = false;
            string errorMessage = string.Empty;
            var exchangeApi = ExchangeServiceHelper.GetExchangeService(ExchangeServiceType.SubscribeAccountInfo);
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolsCacheFactory = SymbolsCacheFactoryHelper.GetSymbolsCachefactory(exchangeService);
            var accountViewModel = new AccountBalancesViewModel(exchangeService, symbolsCacheFactory, new DebugLogger());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            accountViewModel.Account = account;

            var accountObservable = Observable.FromEventPattern<AccountEventArgs>(
                eventHandler => accountViewModel.OnAccountNotification += eventHandler,
                eventHandler => accountViewModel.OnAccountNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            accountObservable.Subscribe(args =>
            {
                if (args.AccountEventType.Equals(AccountEventType.LoggedIn))
                {
                    // expected
                }
                else if (args.AccountEventType.Equals(AccountEventType.UpdateOrders))
                {
                    hasUpdated = true;
                }
                else
                {
                    fail = true;
                }
            });

            // Act
            await accountViewModel.Login(account);

            // Assert
            var btc = accountViewModel.Account.Balances.SingleOrDefault(ab => ab.Asset.Equals("BTC"));
            var bcpt = accountViewModel.Account.Balances.SingleOrDefault(ab => ab.Asset.Equals("BCPT"));
            var test = accountViewModel.Account.Balances.SingleOrDefault(ab => ab.Asset.Equals("TEST"));

            Assert.IsTrue(hasUpdated);
            Assert.IsNull(btc);
            Assert.IsNull(bcpt);
            Assert.IsNotNull(test);
            Assert.IsTrue(accountViewModel.Account.Balances.Count().Equals(5));
            Assert.IsFalse(fail);
        }

        [TestMethod]
        public async Task SelectedAsset()
        {
            // Arrange
            var fail = false;
            AccountBalance selectedAccountBalance = null;
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolsCacheFactory = SymbolsCacheFactoryHelper.GetSymbolsCachefactory(exchangeService);
            var accountViewModel = new AccountBalancesViewModel(exchangeService, symbolsCacheFactory, new DebugLogger());

            var account = new Account(new Core.Model.AccountInfo { User = new Core.Model.User() })
            {
                ApiKey = "apikey",
                ApiSecret = "apisecret"
            };

            accountViewModel.Account = account;

            var accountObservable = Observable.FromEventPattern<AccountEventArgs>(
                eventHandler => accountViewModel.OnAccountNotification += eventHandler,
                eventHandler => accountViewModel.OnAccountNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            accountObservable.Subscribe(args =>
            {
                if (args.AccountEventType.Equals(AccountEventType.LoggedIn))
                {
                    // expected
                }
                else if (args.AccountEventType.Equals(AccountEventType.SelectedAsset))
                {
                    selectedAccountBalance = args.SelectedAsset;
                }
                else
                {
                    fail = true;
                }
            });

            // Act
            await accountViewModel.Login(account);
            var trx = accountViewModel.Account.Balances.Single(ab => ab.Asset.Equals("TRX"));
            accountViewModel.SelectedAsset = trx;

            // Assert
            Assert.AreEqual(selectedAccountBalance, trx);
            Assert.IsFalse(fail);
        }
    }
}
