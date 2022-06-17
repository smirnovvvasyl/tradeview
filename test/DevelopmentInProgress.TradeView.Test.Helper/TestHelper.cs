﻿using DevelopmentInProgress.TradeView.Core.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevelopmentInProgress.TradeView.Test.Helper
{
    public static class TestHelper
    {
        private static readonly string symbols;
        private static readonly string symbolsStatistics;
        private static readonly string orders;
        private static readonly string eth;
        private static readonly string ethStats;
        private static readonly string trx;
        private static readonly string trxStats;
        private static readonly string accountInfo;
        private static readonly string trades;
        private static readonly string tradesUpdated;
        private static readonly string aggregateTrades;
        private static readonly string aggregateTradesUpdated;
        private static readonly string orderBook;
        private static readonly string orderBookUpdated;
        private static readonly string bnb;

        static  TestHelper()
        {
            symbols = File.ReadAllText("Symbols.txt");
            symbolsStatistics = File.ReadAllText("SymbolsStatistics.txt");
            orders = File.ReadAllText("Orders.txt");
            accountInfo = File.ReadAllText("AccountInfo.txt");
            aggregateTrades = File.ReadAllText("AggregateTrades.txt");
            aggregateTradesUpdated = File.ReadAllText("AggregateTradesUpdated.txt");
            trades = File.ReadAllText("Trades.txt");
            tradesUpdated = File.ReadAllText("TradesUpdated.txt");
            orderBook = File.ReadAllText("OrderBook.txt");
            orderBookUpdated = File.ReadAllText("OrderBookUpdated.txt");

            var e = Symbols.Single(s => s.BaseAsset.Symbol.Equals("ETH") && s.QuoteAsset.Symbol.Equals("BTC"));
            eth = JsonConvert.SerializeObject(e);

            var es = SymbolsStatistics.Single(s => s.Symbol.Equals("ETHBTC"));
            ethStats = JsonConvert.SerializeObject(es);

            var t = Symbols.Single(s => s.BaseAsset.Symbol.Equals("TRX") && s.QuoteAsset.Symbol.Equals("BTC"));
            trx = JsonConvert.SerializeObject(t);

            var ts = SymbolsStatistics.Single(s => s.Symbol.Equals("TRXBTC"));
            trxStats = JsonConvert.SerializeObject(ts);

            var b = Symbols.Single(s => s.BaseAsset.Symbol.Equals("BNB") && s.QuoteAsset.Symbol.Equals("BTC"));
            bnb = JsonConvert.SerializeObject(b);
        }
        
        public static List<Symbol> Symbols
        {
            get
            {
                var results = JsonConvert.DeserializeObject<List<Symbol>>(symbols);
                foreach(var s in results)
                {
                    s.Name = $"{s.BaseAsset.Symbol}{s.QuoteAsset.Symbol}";
                    s.ExchangeSymbol = $"{s.BaseAsset.Symbol}{s.QuoteAsset.Symbol}";
                }

                return results;
            }
        }

        public static List<SymbolStats> SymbolsStatistics
        {
            get
            {
                return JsonConvert.DeserializeObject<List<SymbolStats>>(symbolsStatistics);
            }
        }

        public static List<Order> Orders
        {
            get
            {
                return JsonConvert.DeserializeObject<List<Order>>(orders);
            }
        }

        public static OrderBook OrderBook
        {
            get
            {
                return JsonConvert.DeserializeObject<OrderBook>(orderBook);
            }
        }

        public static OrderBook KucoinOrderBook_Update_RestCreate
        {
            get
            {
                return new OrderBook
                {
                    LastUpdateId = 100,
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 0.13m, Quantity = 13 },
                        new  OrderBookPriceLevel { Price = 0.15m, Quantity = 1 },
                        new  OrderBookPriceLevel { Price = 0.16m, Quantity = 1 }
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 0.07m, Quantity = 7 },
                        new  OrderBookPriceLevel { Price = 0.05m, Quantity = 1 },
                        new  OrderBookPriceLevel { Price = 0.04m, Quantity = 4 }
                    }
                };
            }
        }

        public static OrderBook KucoinOrderBook_Update_RestReplay
        {
            get
            {
                return new OrderBook
                {
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 0.13m, Quantity = 0, Id = 101 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 0.14m, Quantity = 14, Id = 102 },   // INSERT
                        new  OrderBookPriceLevel { Price = 0.15m, Quantity = 15, Id = 105 },   // UPDATE
                        new  OrderBookPriceLevel { Price = 0.18m, Quantity = 18, Id = 106 }    // ADD
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 0.07m, Quantity = 0, Id = 107 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 0.06m, Quantity = 1, Id = 108 },    // INSERT
                        new  OrderBookPriceLevel { Price = 0.05m, Quantity = 5, Id = 109 },    // UPDATE
                        new  OrderBookPriceLevel { Price = 0.02m, Quantity = 2, Id = 112 }     // ADD
                    }
                };
            }
        }

        public static OrderBook KucoinOrderBook_Update
        {
            get
            {
                return new OrderBook
                {
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 0.14m, Quantity = 0, Id = 113 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 0.16m, Quantity = 16, Id = 114 },   // UPDATE
                        new  OrderBookPriceLevel { Price = 0.17m, Quantity = 17, Id = 115 },   // INSERT
                        new  OrderBookPriceLevel { Price = 0.19m, Quantity = 19, Id = 116 }    // ADD
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 0.06m, Quantity = 6, Id = 117 },    // UPDATE
                        new  OrderBookPriceLevel { Price = 0.05m, Quantity = 0, Id = 118 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 0.03m, Quantity = 3, Id = 119 },    // INSERT
                        new  OrderBookPriceLevel { Price = 0.01m, Quantity = 1, Id = 120 }     // ADD
                    }
                };
            }
        }

        public static OrderBook KucoinOrderBook_Create_Rest
        {
            get
            {
                return new OrderBook
                {
                    LastUpdateId = 100,
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 123.20m, Quantity = 20 },
                        new  OrderBookPriceLevel { Price = 123.21m, Quantity = 21 },
                        new  OrderBookPriceLevel { Price = 123.24m, Quantity = 24 },
                        new  OrderBookPriceLevel { Price = 123.25m, Quantity = 25 },
                        new  OrderBookPriceLevel { Price = 123.26m, Quantity = 26 },
                        new  OrderBookPriceLevel { Price = 123.27m, Quantity = 27 }
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 123.17m, Quantity = 17 },
                        new  OrderBookPriceLevel { Price = 123.16m, Quantity = 16 },
                        new  OrderBookPriceLevel { Price = 123.13m, Quantity = 13 },
                        new  OrderBookPriceLevel { Price = 123.12m, Quantity = 12 },
                        new  OrderBookPriceLevel { Price = 123.11m, Quantity = 11 },
                        new  OrderBookPriceLevel { Price = 123.10m, Quantity = 10 }
                    }
                };
            }
        }

        public static OrderBook KucoinOrderBook_Create_RUIRRA
        {
            get
            {
                return new OrderBook
                {
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 123.20m, Quantity = 0, Id = 101 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 123.21m, Quantity = 211, Id = 102 },  // UPDATE
                        new  OrderBookPriceLevel { Price = 123.22m, Quantity = 22, Id = 105 },   // INSERT
                        new  OrderBookPriceLevel { Price = 123.24m, Quantity = 0, Id = 103 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 123.25m, Quantity = 0, Id = 104 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 123.28m, Quantity = 28, Id = 106 }    // ADD
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 123.17m, Quantity = 0, Id = 107 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 123.16m, Quantity = 160, Id = 108 },  // UPDATE
                        new  OrderBookPriceLevel { Price = 123.15m, Quantity = 15, Id = 109 },   // INSERT
                        new  OrderBookPriceLevel { Price = 123.13m, Quantity = 0, Id = 110 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 123.12m, Quantity = 0, Id = 111 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 123.09m, Quantity = 09, Id = 112 }    // ADD
                    }
                };
            }
        }

        public static OrderBook KucoinOrderBook_Create_IUIIRA
        {
            get
            {
                return new OrderBook
                {
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 123.19m, Quantity = 19, Id = 101 },   // INSERT
                        new  OrderBookPriceLevel { Price = 123.21m, Quantity = 211, Id = 102 },  // UPDATE
                        new  OrderBookPriceLevel { Price = 123.22m, Quantity = 22, Id = 103 },   // INSERT
                        new  OrderBookPriceLevel { Price = 123.23m, Quantity = 23, Id = 104 },   // INSERT
                        new  OrderBookPriceLevel { Price = 123.25m, Quantity = 0, Id = 105 },    // REMOVE
                        new  OrderBookPriceLevel { Price = 123.28m, Quantity = 28, Id = 106 }    // ADD
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 123.18m, Quantity = 18, Id = 107 },    // INSERT
                        new  OrderBookPriceLevel { Price = 123.16m, Quantity = 160, Id = 108 },   // UPDATE
                        new  OrderBookPriceLevel { Price = 123.15m, Quantity = 15, Id = 109 },    // INSERT
                        new  OrderBookPriceLevel { Price = 123.14m, Quantity = 14, Id = 110 },    // INSERT
                        new  OrderBookPriceLevel { Price = 123.12m, Quantity = 0, Id = 111 },     // REMOVE
                        new  OrderBookPriceLevel { Price = 123.09m, Quantity = 09, Id = 112 }     // ADD
                    }
                };
            }
        }

        public static OrderBook KucoinOrderBook_16
        {
            get
            {
                return new OrderBook
                {
                    LastUpdateId = 16,
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 3988.59m, Quantity = 3 },
                        new  OrderBookPriceLevel { Price = 3988.60m, Quantity = 47 },
                        new  OrderBookPriceLevel { Price = 3988.61m, Quantity = 32 },
                        new  OrderBookPriceLevel { Price = 3988.62m, Quantity = 8 }
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 3988.51m, Quantity = 56 },
                        new  OrderBookPriceLevel { Price = 3988.50m, Quantity = 15 },
                        new  OrderBookPriceLevel { Price = 3988.49m, Quantity = 100 },
                        new  OrderBookPriceLevel { Price = 3988.48m, Quantity = 10 }
                    }
                };
            }
        }

        public static OrderBook KucoinOrderBook_15_18
        {
            get
            {
                return new OrderBook
                {
                    Asks = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 3988.59m, Quantity = 3, Id = 16 },
                        new  OrderBookPriceLevel { Price = 3988.61m, Quantity = 0, Id = 18 },
                        new  OrderBookPriceLevel { Price = 3988.62m, Quantity = 8, Id = 15 }
                    },
                    Bids = new List<OrderBookPriceLevel>
                    {
                        new  OrderBookPriceLevel { Price = 3988.50m, Quantity = 44, Id = 17 }
                    }
                };
            }
        }

        public static OrderBook OrderBookUpdated
        {
            get
            {
                return JsonConvert.DeserializeObject<OrderBook>(orderBookUpdated);
            }
        }

        public static List<AggregateTrade> AggregateTrades
        {
            get
            {
                return JsonConvert.DeserializeObject<List<AggregateTrade>>(aggregateTrades);
            }
        }

        public static List<AggregateTrade> AggregateTradesUpdated
        {
            get
            {
                return JsonConvert.DeserializeObject<List<AggregateTrade>>(aggregateTradesUpdated);
            }
        }

        public static List<Trade> Trades
        {
            get
            {
                return JsonConvert.DeserializeObject<List<Trade>>(trades);
            }
        }

        public static List<Trade> TradesUpdated
        {
            get
            {
                return JsonConvert.DeserializeObject<List<Trade>>(tradesUpdated);
            }
        }

        public static AccountInfo AccountInfo
        {
            get
            {
                return JsonConvert.DeserializeObject<AccountInfo>(accountInfo);
            }
        }

        public static Symbol Trx
        {
            get
            {
                var symbol = JsonConvert.DeserializeObject<Symbol>(trx);
                symbol.SymbolStatistics = JsonConvert.DeserializeObject<SymbolStats>(trxStats);
                return symbol;
            }
        }

        public static Symbol BNB
        {
            get
            {
                var symbol = JsonConvert.DeserializeObject<Symbol>(bnb);
                symbol.SymbolStatistics = new SymbolStats { Symbol = symbol.ExchangeSymbol };
                return symbol;
            }
        }

        public static SymbolStats TrxStats
        {
            get
            {
                return JsonConvert.DeserializeObject<SymbolStats>(trxStats);
            }
        }

        public static Symbol Eth
        {
            get
            {
                var symbol = JsonConvert.DeserializeObject<Symbol>(eth);
                symbol.SymbolStatistics = JsonConvert.DeserializeObject<SymbolStats>(ethStats);
                return symbol;
            }
        }

        public static SymbolStats EthStats
        {
            get
            {
                return JsonConvert.DeserializeObject<SymbolStats>(ethStats);
            }
        }

        public static SymbolStats EthStats_UpdatedLastPrice_Upwards
        {
            get
            {
                var origEthStats = EthStats;
                var updatedEthStats = EthStats;

                updatedEthStats.PriceChange = 0.00156M;
                updatedEthStats.LastPrice = origEthStats.LastPrice + updatedEthStats.PriceChange;

                return updatedEthStats;
            }
        }

        public static SymbolStats EthStats_UpdatedLastPrice_Downwards
        {
            get
            {
                var origEthStats = EthStats;
                var updatedEthStats = EthStats;

                updatedEthStats.PriceChange = 0.00156M;
                updatedEthStats.LastPrice = origEthStats.LastPrice - updatedEthStats.PriceChange;

                return updatedEthStats;
            }
        }
    }
}
