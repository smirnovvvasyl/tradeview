﻿using DevelopmentInProgress.TradeView.Core.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevelopmentInProgress.TradeServer.StrategyExecution.WebHost.Notification.Server
{
    public interface IServerNotificationPublisher
    {
        Task PublishNotificationsAsync(IEnumerable<ServerNotification> notifications);
    }
}
