﻿using DevelopmentInProgress.TradeView.Core.Enums;
using DevelopmentInProgress.TradeView.Core.Model;
using DevelopmentInProgress.TradeView.Core.TradeStrategy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopmentInProgress.TradeView.Data.File
{
    public class TradeViewConfigurationStrategyFile : ITradeViewConfigurationStrategy
    {
        private readonly string userStrategiesFile;

        public TradeViewConfigurationStrategyFile()
        {
            userStrategiesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Environment.UserName}_Strategies.txt");
        }

        public async Task<List<StrategyConfig>> GetStrategiesAsync()
        {
            if (System.IO.File.Exists(userStrategiesFile))
            {
                using var reader = System.IO.File.OpenText(userStrategiesFile);
                var json = await reader.ReadToEndAsync().ConfigureAwait(true);
                var strategies = JsonConvert.DeserializeObject<List<StrategyConfig>>(json);
                return strategies;
            }

            return new List<StrategyConfig> 
            {
                GetDemoStrategyConfig()
            };
        }

        public async Task<StrategyConfig> GetStrategyAsync(string strategyName)
        {
            StrategyConfig strategy = null;

            if (System.IO.File.Exists(userStrategiesFile))
            {
                using var reader = System.IO.File.OpenText(userStrategiesFile);
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                var strategies = JsonConvert.DeserializeObject<List<StrategyConfig>>(json);
                strategy = strategies.FirstOrDefault(s => s.Name.Equals(strategyName, StringComparison.Ordinal));
                return strategy;
            }

            return GetDemoStrategyConfig();
        }

        public async Task SaveStrategyAsync(StrategyConfig strategyConfig)
        {
            if (strategyConfig == null)
            {
                return;
            }

            List<StrategyConfig> strategies;

            if (System.IO.File.Exists(userStrategiesFile))
            {
                using var reader = System.IO.File.OpenText(userStrategiesFile);
                var rjson = await reader.ReadToEndAsync().ConfigureAwait(false);
                strategies = JsonConvert.DeserializeObject<List<StrategyConfig>>(rjson);
            }
            else
            {
                strategies = new List<StrategyConfig>();
            }

            var dupe = strategies.FirstOrDefault(s => s.Name.Equals(strategyConfig.Name, StringComparison.Ordinal));
            if (dupe != null)
            {
                strategies.Remove(dupe);
            }

            strategies.Add(strategyConfig);

            var wjson = JsonConvert.SerializeObject(strategies, Formatting.Indented);

            UnicodeEncoding encoding = new UnicodeEncoding();
            char[] chars = encoding.GetChars(encoding.GetBytes(wjson));
            using StreamWriter writer = System.IO.File.CreateText(userStrategiesFile);
            await writer.WriteAsync(chars, 0, chars.Length).ConfigureAwait(false);
        }

        public async Task DeleteStrategyAsync(StrategyConfig strategyConfig)
        {
            if (System.IO.File.Exists(userStrategiesFile))
            {
                List<StrategyConfig> strategies = null;

                using (var reader = System.IO.File.OpenText(userStrategiesFile))
                {
                    var rjson = await reader.ReadToEndAsync().ConfigureAwait(false);
                    strategies = JsonConvert.DeserializeObject<List<StrategyConfig>>(rjson);
                }

                var remove = strategies.FirstOrDefault(s => s.Name.Equals(strategyConfig.Name, StringComparison.Ordinal));
                if (remove != null)
                {
                    strategies.Remove(remove);
                    var wjson = JsonConvert.SerializeObject(strategies);

                    UnicodeEncoding encoding = new UnicodeEncoding();
                    char[] chars = encoding.GetChars(encoding.GetBytes(wjson));
                    using StreamWriter writer = System.IO.File.CreateText(userStrategiesFile);
                    await writer.WriteAsync(chars, 0, chars.Length).ConfigureAwait(false);
                }
            }
        }

        private static StrategyConfig GetDemoStrategyConfig()
        {
            var strategyConfig = new StrategyConfig
            {
                Name = "Binance Moving Average - ETHBTC",
                TargetType = "DevelopmentInProgress.Strategy.MovingAverage.MovingAverageStrategy",
                TargetAssembly = Path.Combine(Environment.CurrentDirectory, "DevelopmentInProgress.Strategy.MovingAverage.dll"),
                DisplayViewType = "DevelopmentInProgress.Strategy.MovingAverage.Wpf.View.MovingAverageView",
                DisplayViewModelType = "DevelopmentInProgress.Strategy.MovingAverage.Wpf.ViewModel.MovingAverageViewModel",
                DisplayAssembly = Path.Combine(Environment.CurrentDirectory, "DevelopmentInProgress.Strategy.MovingAverage.Wpf.dll"),
                Parameters = "{\r\n  \"TradeRange\": 10000,\r\n  \"BuyIndicator\": 0.00015,\r\n  \"SellIndicator\": 0.00015,\r\n  \"MovingAvarageRange\": 10000,\r\n  \"Suspend\": true,\r\n  \"StrategyName\": \"Binance Moving Average - ETHBTC\",\r\n  \"Value\": null\r\n}",
                TradesChartDisplayCount = 1000,
                TradesDisplayCount = 13,
                OrderBookChartDisplayCount = 20,
                OrderBookDisplayCount = 9
            };

            strategyConfig.StrategySubscriptions.AddRange(new List<StrategySubscription>
                {
                    new StrategySubscription
                    {
                        AccountName = "Binance - Investment",
                        Symbol = "ETHBTC",
                        Limit = 0,
                        Exchange = Exchange.Binance,
                        Subscribes = Subscribes.Trades | Subscribes.OrderBook | Subscribes.Candlesticks,
                        CandlestickInterval = CandlestickInterval.Minute
                    }
                });

            strategyConfig.Dependencies.AddRange(new List<string>
                {
                    Path.Combine(Environment.CurrentDirectory, @"..\netstandard2.1\DevelopmentInProgress.Strategy.Common.dll"),
                    Path.Combine(Environment.CurrentDirectory, @"..\netstandard2.1\DevelopmentInProgress.TradeView.Core.dll"),
                    Path.Combine(Environment.CurrentDirectory, @"..\netstandard2.1\DevelopmentInProgress.Strategy.MovingAverage.dll"),
                });

            strategyConfig.DisplayDependencies.AddRange(new List<string>
                {
                    Path.Combine(Environment.CurrentDirectory, "DevelopmentInProgress.TradeView.Core.dll"),
                    Path.Combine(Environment.CurrentDirectory, "DevelopmentInProgress.Strategy.MovingAverage.Wpf.dll"),
                    Path.Combine(Environment.CurrentDirectory, @"..\netstandard2.1\DevelopmentInProgress.Strategy.MovingAverage.dll")
                });

            return strategyConfig;
        }
    }
}
