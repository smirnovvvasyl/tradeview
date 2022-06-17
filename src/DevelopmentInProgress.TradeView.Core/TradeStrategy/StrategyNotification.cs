﻿using Newtonsoft.Json;
using System;

namespace DevelopmentInProgress.TradeView.Core.TradeStrategy
{
    public class StrategyNotification : Strategy
    {
        public string Machine { get; set; }
        public string Message { get; set; }
        public string MethodName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public NotificationLevel NotificationLevel { get; set; }
        public int NotificationEvent { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }
    }
}