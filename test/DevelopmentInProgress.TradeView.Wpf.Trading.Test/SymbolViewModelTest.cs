﻿using DevelopmentInProgress.TradeView.Core.Enums;
using DevelopmentInProgress.TradeView.Core.Interfaces;
using DevelopmentInProgress.TradeView.Core.Model;
using DevelopmentInProgress.TradeView.Test.Helper;
using DevelopmentInProgress.TradeView.Wpf.Common.Chart;
using DevelopmentInProgress.TradeView.Wpf.Common.Extensions;
using DevelopmentInProgress.TradeView.Wpf.Common.Helpers;
using DevelopmentInProgress.TradeView.Wpf.Common.Services;
using DevelopmentInProgress.TradeView.Wpf.Trading.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model = DevelopmentInProgress.TradeView.Wpf.Common.Model;

namespace DevelopmentInProgress.TradeView.Wpf.Trading.Test
{
    [TestClass]
    public class SymbolViewModelTest
    {
        IChartHelper chartHelper;

        [TestInitialize]
        public void TestInitialize()
        {
            chartHelper = new ChartHelper();
        }

        [TestMethod]
        public async Task SetSymbol()
        {
            // Arrange
            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5,
                TradesDisplayCount = 5,
                TradesChartDisplayCount = 8
            };

            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(), 
                new TradeHelper(),
                preferences, new DebugLogger());

            var trx = TestHelper.Trx.GetViewSymbol();
            
            // Act
            await symbolViewModel.SetSymbol(trx);

            // Assert
            Assert.AreEqual(symbolViewModel.Symbol, trx);
            Assert.IsNotNull(symbolViewModel.OrderBook);
            Assert.AreEqual(symbolViewModel.OrderBook.LastUpdateId, TestHelper.OrderBook.LastUpdateId);
            Assert.IsTrue(symbolViewModel.OrderBook.TopAsks.Count > 0);
            Assert.IsTrue(symbolViewModel.OrderBook.TopBids.Count > 0);
            Assert.IsTrue(symbolViewModel.Trades.Count > 0);
        }

        [TestMethod]
        public void UpdateOrderBook_FirstUpdate()
        {
            // Arrange
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);

            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5
            };

            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(),
                new TradeHelper(), 
                preferences, new DebugLogger());

            var trx = TestHelper.Trx.GetViewSymbol();
            symbolViewModel.Symbol = trx;

            var orderBook1 = new OrderBookUpdateHelper();
            var orderBook = orderBook1.OrderBook_Trx_GetFirstUpdate();

            // Act
            symbolViewModel.UpdateOrderBook(orderBook);

            // Assert
            AssertOrderBookUpdate(symbolViewModel, orderBook, preferences);
        }

        [TestMethod]
        public void UpdateOrderBook_SecondUpdate()
        {
            // Arrange
            var exchangeApi = ExchangeServiceHelper.GetExchangeService();
            var exchangeService = new WpfExchangeService(exchangeApi);

            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5
            };

            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(),
                new TradeHelper(), 
                preferences, new DebugLogger());

            var trx = TestHelper.Trx.GetViewSymbol();
            symbolViewModel.Symbol = trx;

            var orderBook = new OrderBookUpdateHelper();
            var firstOrderBook = orderBook.OrderBook_Trx_GetFirstUpdate();
            var secondOrderBook = orderBook.OrderBook_Trx_GetSecondUpdate();

            // AppVeyor Hack!!!
            if(firstOrderBook.LastUpdateId.Equals(secondOrderBook.LastUpdateId))
            {
                secondOrderBook.LastUpdateId++;
            }

            // Act
            symbolViewModel.UpdateOrderBook(firstOrderBook);

            symbolViewModel.UpdateOrderBook(secondOrderBook);

            // Assert
            AssertOrderBookUpdate(symbolViewModel, secondOrderBook, preferences);
        }

        [TestMethod]
        public void UpdateTrades_First_Update()
        {
            // Arrange
            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5,
                TradesDisplayCount = 5,
                TradesChartDisplayCount = 8
            };

            var exchangeApi = ExchangeServiceHelper.GetExchangeService(ExchangeServiceType.SubscribeOrderBookAggregateTrades);
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(),
                new TradeHelper(), 
                preferences, new DebugLogger());

            var trx = TestHelper.BNB.GetViewSymbol();
            symbolViewModel.Symbol = trx;

            var firstTrades = TradesUpdateHelper.Trades_BNB_InitialTradeUpdate_10_Trades();

            // Act
            symbolViewModel.UpdateTrades(firstTrades);

            // Assert
            AssertTradeUpdate(symbolViewModel, preferences, firstTrades);
        }

        [TestMethod]
        public void UpdateTrades_Second_Update_5_New_Trades()
        {
            // Arrange
            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5,
                TradesDisplayCount = 5,
                TradesChartDisplayCount = 8
            };

            var exchangeApi = ExchangeServiceHelper.GetExchangeService(ExchangeServiceType.SubscribeOrderBookAggregateTrades);
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(),
                new TradeHelper(), 
                preferences, new DebugLogger());

            var trx = TestHelper.BNB.GetViewSymbol();
            symbolViewModel.Symbol = trx;

            var firstTrades = TradesUpdateHelper.Trades_BNB_InitialTradeUpdate_10_Trades();

            var secondTrades = TradesUpdateHelper.Trades_BNB_NextTradeUpdate(firstTrades, 5, 5);

            // Act
            symbolViewModel.UpdateTrades(firstTrades);

            symbolViewModel.UpdateTrades(secondTrades);

            // Assert
            AssertTradeUpdate(symbolViewModel, preferences, secondTrades);
        }

        [TestMethod]
        public void UpdateTrades_Second_Update_3_New_Trades()
        {
            // Arrange
            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5,
                TradesDisplayCount = 5,
                TradesChartDisplayCount = 8
            };

            var exchangeApi = ExchangeServiceHelper.GetExchangeService(ExchangeServiceType.SubscribeOrderBookAggregateTrades);
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(),
                new TradeHelper(), 
                preferences, new DebugLogger());

            var trx = TestHelper.BNB.GetViewSymbol();
            symbolViewModel.Symbol = trx;

            var firstTrades = TradesUpdateHelper.Trades_BNB_InitialTradeUpdate_10_Trades();

            var secondTrades = TradesUpdateHelper.Trades_BNB_NextTradeUpdate(firstTrades, 3, 3);

            // Act
            symbolViewModel.UpdateTrades(firstTrades);

            symbolViewModel.UpdateTrades(secondTrades);

            // Assert
            AssertTradeUpdate(symbolViewModel, preferences, secondTrades);
        }

        [TestMethod]
        public void UpdateTrades_Second_Update_3_New_Trades_Less_Than_Limit()
        {
            // Arrange
            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5,
                TradesDisplayCount = 15,
                TradesChartDisplayCount = 18
            };

            var exchangeApi = ExchangeServiceHelper.GetExchangeService(ExchangeServiceType.SubscribeOrderBookAggregateTrades);
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(),
                new TradeHelper(), 
                preferences, new DebugLogger());

            var trx = TestHelper.BNB.GetViewSymbol();
            symbolViewModel.Symbol = trx;

            var firstTrades = TradesUpdateHelper.Trades_BNB_InitialTradeUpdate_10_Trades();

            var secondTrades = TradesUpdateHelper.Trades_BNB_NextTradeUpdate(firstTrades, 3, 3);

            // Act
            symbolViewModel.UpdateTrades(firstTrades);

            symbolViewModel.UpdateTrades(secondTrades);

            // Assert
            Assert.AreEqual(symbolViewModel.Trades.Count, 13);
            Assert.AreEqual(symbolViewModel.TradesChart.Count, 13);

            var chart = firstTrades.Take(3).ToList();
            chart.AddRange(secondTrades);

            // Assert - chart trades
            for (int i = 0; i < 13; i++)
            {
                Assert.AreEqual(symbolViewModel.TradesChart[i].Id, chart[i].Id);
                Assert.AreEqual(symbolViewModel.TradesChart[i].Time, chart[i].Time);
            }

            // Assert - trades
            var trades = chart.Reverse<ITrade>().ToList();
            for (int i = 0; i < 13; i++)
            {
                Assert.AreEqual(symbolViewModel.Trades[i].Id, trades[i].Id);
                Assert.AreEqual(symbolViewModel.Trades[i].Time, trades[i].Time);
            }
        }

        [TestMethod]
        public void UpdateTrades_Third_Update()
        {
            // Arrange
            var preferences = new Model.Preferences
            {
                OrderBookChartDisplayCount = 8,
                OrderBookDisplayCount = 5,
                TradesDisplayCount = 15,
                TradesChartDisplayCount = 18
            };

            var exchangeApi = ExchangeServiceHelper.GetExchangeService(ExchangeServiceType.SubscribeOrderBookAggregateTrades);
            var exchangeService = new WpfExchangeService(exchangeApi);
            var symbolViewModel = new SymbolViewModel(Exchange.Test, exchangeService, chartHelper, 
                new BinanceOrderBookHelper(),
                new TradeHelper(), 
                preferences, new DebugLogger());

            var trx = TestHelper.BNB.GetViewSymbol();
            symbolViewModel.Symbol = trx;

            var firstTrades = TradesUpdateHelper.Trades_BNB_InitialTradeUpdate_10_Trades();

            var secondTrades = TradesUpdateHelper.Trades_BNB_NextTradeUpdate(firstTrades, 3, 3);

            var thirdTrades = TradesUpdateHelper.Trades_BNB_NextTradeUpdate(secondTrades, 9, 9);

            // Act
            symbolViewModel.UpdateTrades(firstTrades);

            symbolViewModel.UpdateTrades(secondTrades);

            symbolViewModel.UpdateTrades(thirdTrades);

            // Assert
            var update = secondTrades.Skip(1).Take(8).ToList();
            update.AddRange(thirdTrades);

            AssertTradeUpdate(symbolViewModel, preferences, update);
        }

        private void AssertTradeUpdate(SymbolViewModel symbolViewModel, Model.Preferences preferences, List<ITrade> trades)
        {
            Assert.AreEqual(symbolViewModel.Trades.Count, preferences.TradesDisplayCount);
            Assert.AreEqual(symbolViewModel.TradesChart.Count, preferences.TradesChartDisplayCount);

            // Assert - trades
            var lastTrades = trades.Skip(trades.Count - preferences.TradesDisplayCount).ToList();

            var lastTradesReversed = lastTrades.Reverse<ITrade>().ToList();

            for (int i = 0; i < preferences.TradesDisplayCount; i++)
            {
                Assert.AreEqual(symbolViewModel.Trades[i].Id, lastTradesReversed[i].Id);
                Assert.AreEqual(symbolViewModel.Trades[i].Time, lastTradesReversed[i].Time);
            }

            // Assert - chart trades
            var lastChartTrades = trades.Skip(trades.Count - preferences.TradesChartDisplayCount).ToList();

            for (int i = 0; i < preferences.TradesChartDisplayCount; i++)
            {
                Assert.AreEqual(symbolViewModel.TradesChart[i].Id, lastChartTrades[i].Id);
                Assert.AreEqual(symbolViewModel.TradesChart[i].Time, lastChartTrades[i].Time);
            }
        }

        private void AssertOrderBookUpdate(SymbolViewModel symbolViewModel, OrderBook orderBook, Model.Preferences preferences)
        {
            // Assert - Last Update Id
            Assert.AreEqual(symbolViewModel.OrderBook.LastUpdateId, orderBook.LastUpdateId);

            // Assert - TopAsks
            Assert.AreEqual(symbolViewModel.OrderBook.TopAsks.Count, preferences.OrderBookDisplayCount);
            var topAsks = orderBook.Asks.Take(preferences.OrderBookDisplayCount).Reverse().ToList();
            for (int i = 0; i < preferences.OrderBookDisplayCount; i++)
            {
                Assert.AreEqual(symbolViewModel.OrderBook.TopAsks[i].Price, topAsks[i].Price);
                Assert.AreEqual(symbolViewModel.OrderBook.TopAsks[i].Quantity, topAsks[i].Quantity);
            }

            // Assert - TopBids
            Assert.AreEqual(symbolViewModel.OrderBook.TopBids.Count, preferences.OrderBookDisplayCount);
            var topBids = orderBook.Bids.Take(preferences.OrderBookDisplayCount).ToList();
            for (int i = 0; i < preferences.OrderBookDisplayCount; i++)
            {
                Assert.AreEqual(symbolViewModel.OrderBook.TopBids[i].Price, topBids[i].Price);
                Assert.AreEqual(symbolViewModel.OrderBook.TopBids[i].Quantity, topBids[i].Quantity);
            }

            // Assert - ChartAsks
            Assert.AreEqual(symbolViewModel.OrderBook.ChartAsks.Count, preferences.OrderBookChartDisplayCount);
            var chartAsks = orderBook.Asks.Take(preferences.OrderBookChartDisplayCount).ToList();
            for (int i = 0; i < preferences.OrderBookChartDisplayCount; i++)
            {
                Assert.AreEqual(symbolViewModel.OrderBook.ChartAsks[i].Price, chartAsks[i].Price);
                Assert.AreEqual(symbolViewModel.OrderBook.ChartAsks[i].Quantity, chartAsks[i].Quantity);
            }

            // Assert ChartBids
            Assert.AreEqual(symbolViewModel.OrderBook.ChartBids.Count, preferences.OrderBookChartDisplayCount);
            var chartBids = orderBook.Bids.Take(preferences.OrderBookChartDisplayCount).Reverse<OrderBookPriceLevel>().ToList();
            for (int i = 0; i < preferences.OrderBookChartDisplayCount; i++)
            {
                Assert.AreEqual(symbolViewModel.OrderBook.ChartBids[i].Price, chartBids[i].Price);
                Assert.AreEqual(symbolViewModel.OrderBook.ChartBids[i].Quantity, chartBids[i].Quantity);
            }

            // Assert ChartAggregateAsks
            Assert.AreEqual(symbolViewModel.OrderBook.ChartAggregatedAsks.Count, preferences.OrderBookChartDisplayCount);
            var runningTotal = 0m;
            for (int i = 0; i < preferences.OrderBookChartDisplayCount; i++)
            {
                if (i == 0)
                {
                    runningTotal = chartAsks[i].Quantity;
                }
                else
                {
                    runningTotal = chartAsks[i].Quantity + runningTotal;
                }

                Assert.AreEqual(symbolViewModel.OrderBook.ChartAggregatedAsks[i].Price, chartAsks[i].Price);
                Assert.AreEqual(symbolViewModel.OrderBook.ChartAggregatedAsks[i].Quantity, runningTotal);
            }

            // Assert ChartAggregateBids
            Assert.AreEqual(symbolViewModel.OrderBook.ChartAggregatedBids.Count, preferences.OrderBookChartDisplayCount);

            var aggregatedBidsList = orderBook.Bids.Take(preferences.OrderBookChartDisplayCount).Select(p => new OrderBookPriceLevel { Price = p.Price, Quantity = p.Quantity }).ToList();
            for (int i = 0; i < preferences.OrderBookChartDisplayCount; i++)
            {
                if (i > 0)
                {
                    aggregatedBidsList[i].Quantity = aggregatedBidsList[i].Quantity + aggregatedBidsList[i - 1].Quantity;
                }
            }

            var reversedAggregateBidsList = aggregatedBidsList.Reverse<OrderBookPriceLevel>().ToList();
            for (int i = 0; i < preferences.OrderBookChartDisplayCount; i++)
            {
                Assert.AreEqual(symbolViewModel.OrderBook.ChartAggregatedBids[i].Price, reversedAggregateBidsList[i].Price);
                Assert.AreEqual(symbolViewModel.OrderBook.ChartAggregatedBids[i].Quantity, reversedAggregateBidsList[i].Quantity);
            }
        }
    }
}
