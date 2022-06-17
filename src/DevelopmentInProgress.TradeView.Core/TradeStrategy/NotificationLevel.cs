﻿namespace DevelopmentInProgress.TradeView.Core.TradeStrategy
{
    public enum NotificationLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Trade = 4,
        TradeError = 5,
        OrderBook = 6,
        OrderBookError = 7,
        Account = 8,
        AccountError = 9,
        Statistics = 10,
        StatisticsError = 11,
        Candlesticks = 12,
        CandlesticksError = 13,
        ParameterUpdate = 14,
        DisconnectClient = 15
    }
}