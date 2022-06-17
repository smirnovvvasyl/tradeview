﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevelopmentInProgress.TradeView.Core.Enums;
using DevelopmentInProgress.TradeView.Core.Model;
using Newtonsoft.Json;

namespace DevelopmentInProgress.TradeView.Data.File
{
    public class TradeViewConfigurationAccountsFile : ITradeViewConfigurationAccounts
    {
        private readonly string userAccountsFile;

        public TradeViewConfigurationAccountsFile()
        {
            userAccountsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Environment.UserName}_Accounts.txt");
        }

        public async Task DeleteAccountAsync(UserAccount userAccount)
        {
            if (System.IO.File.Exists(userAccountsFile))
            {
                UserAccounts userAccounts;

                using (var reader = System.IO.File.OpenText(userAccountsFile))
                {
                    var rjson = await reader.ReadToEndAsync().ConfigureAwait(false);
                    userAccounts = JsonConvert.DeserializeObject<UserAccounts>(rjson);
                }

                var remove = userAccounts.Accounts.FirstOrDefault(a => a.AccountName.Equals(userAccount.AccountName, StringComparison.Ordinal));
                if (remove != null)
                {
                    userAccounts.Accounts.Remove(remove);
                    var wjson = JsonConvert.SerializeObject(userAccounts, Formatting.Indented);

                    UnicodeEncoding encoding = new UnicodeEncoding();
                    char[] chars = encoding.GetChars(encoding.GetBytes(wjson));
                    using StreamWriter writer = System.IO.File.CreateText(userAccountsFile);
                    await writer.WriteAsync(chars, 0, chars.Length).ConfigureAwait(false);
                }
            }
        }

        public async Task<UserAccount> GetAccountAsync(string accountName)
        {
            if (System.IO.File.Exists(userAccountsFile))
            {
                string json;
                using (var reader = System.IO.File.OpenText(userAccountsFile))
                {
                    json = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                var userAccountss = JsonConvert.DeserializeObject<UserAccounts>(json);
                var userAccount = userAccountss.Accounts.Single(a => a.AccountName.Equals(accountName, StringComparison.Ordinal));
                return userAccount;
            }

            return GetDemoAccount();
        }

        public async Task<UserAccounts> GetAccountsAsync()
        {
            if (System.IO.File.Exists(userAccountsFile))
            {
                using var reader = System.IO.File.OpenText(userAccountsFile);
                var json = await reader.ReadToEndAsync().ConfigureAwait(true);
                var userAccounts = JsonConvert.DeserializeObject<UserAccounts>(json);
                return userAccounts;
            }

            var demoUserAccounts = new UserAccounts();
            demoUserAccounts.Accounts.Add(GetDemoAccount());
            return demoUserAccounts;
        }

        public async Task SaveAccountAsync(UserAccount userAccount)
        {
            UserAccounts userAccounts;

            if (System.IO.File.Exists(userAccountsFile))
            {
                using var reader = System.IO.File.OpenText(userAccountsFile);
                var rjson = await reader.ReadToEndAsync().ConfigureAwait(false);
                userAccounts = JsonConvert.DeserializeObject<UserAccounts>(rjson);
            }
            else
            {
                userAccounts = new UserAccounts();
            }

            var dupe = userAccounts.Accounts.FirstOrDefault(a => a.AccountName.Equals(userAccount.AccountName, StringComparison.Ordinal));
            if (dupe != null)
            {
                userAccounts.Accounts.Remove(dupe);
            }

            userAccounts.Accounts.Add(userAccount);

            var wjson = JsonConvert.SerializeObject(userAccounts, Formatting.Indented);

            UnicodeEncoding encoding = new UnicodeEncoding();
            char[] chars = encoding.GetChars(encoding.GetBytes(wjson));
            using StreamWriter writer = System.IO.File.CreateText(userAccountsFile);
            await writer.WriteAsync(chars, 0, chars.Length).ConfigureAwait(false);
        }

        private static UserAccount GetDemoAccount()
        {
            var userAccount = new UserAccount
            {
                AccountName = "Demo Account",
                Exchange = Exchange.Binance,
                Preferences = new Preferences
                {
                    SelectedSymbol = "ETHBTC",
                    TradeLimit = 500,
                    TradesChartDisplayCount = 500,
                    TradesDisplayCount = 16,
                    OrderBookDisplayCount = 9,
                    OrderBookChartDisplayCount = 15
                }
            };

            userAccount.Preferences.FavouriteSymbols.AddRange(new string[] { "BTCUSDT", "ETHBTC", "ETHUSDT" });

            return userAccount;
        }
    }
}
