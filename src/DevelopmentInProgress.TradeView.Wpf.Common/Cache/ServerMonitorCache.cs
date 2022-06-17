﻿using DevelopmentInProgress.TradeView.Data;
using DevelopmentInProgress.TradeView.Wpf.Common.Events;
using DevelopmentInProgress.TradeView.Wpf.Common.Extensions;
using DevelopmentInProgress.TradeView.Wpf.Common.Manager;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DevelopmentInProgress.TradeView.Wpf.Common.Cache
{
    public sealed class ServerMonitorCache : IServerMonitorCache
    {
        private readonly IHttpClientManager httpClientManager;
        private readonly ITradeViewConfigurationServer configurationServer;
        private readonly ObservableCollection<ServerMonitor> serverMonitors;
        private readonly SemaphoreSlim serverMonitorSemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, IDisposable> serverMonitorSubscriptions;
        private readonly Dispatcher dispatcher;
        private Core.Server.ServerConfiguration serverConfiguration;
        private IDisposable observableInterval;
        private bool disposed;

        public ServerMonitorCache(IHttpClientManager httpClientManager, ITradeViewConfigurationServer configurationServer)
        {
            this.httpClientManager = httpClientManager;
            this.configurationServer = configurationServer;
            serverMonitors = new ObservableCollection<ServerMonitor>();
            serverMonitorSubscriptions = new Dictionary<string, IDisposable>();

            dispatcher = Application.Current.Dispatcher;
        }

        public event EventHandler<ServerMonitorCacheEventArgs> ServerMonitorCacheNotification;

        public ObservableCollection<ServerMonitor> GetServerMonitors()
        {
            return serverMonitors;
        }

        public async void Dispose()
        {
            if (disposed)
            {
                return;
            }

            if (observableInterval != null)
            {
                observableInterval.Dispose();
            }

            foreach(var serverMonitorSubscription in serverMonitorSubscriptions.Values)
            {
                serverMonitorSubscription.Dispose();
            }

            await Task.WhenAll(serverMonitors.Select(s => s.DisposeAsync()).ToList()).ConfigureAwait(false);

            if(serverMonitorSemaphoreSlim != null)
            {
                serverMonitorSemaphoreSlim.Dispose();
            }

            disposed = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch all exceptions and notify observers.")]
        public void StartObservingServers()
        {
            if(observableInterval != null)
            {
                return;
            }

            var interval = 0d;

            var onlyOnStartupUntilHttpClientMemoryLeakIsFixed = false;
 
            observableInterval = Observable.Interval(TimeSpan.FromSeconds(interval))
                .Subscribe(async i =>
                {
                    if(onlyOnStartupUntilHttpClientMemoryLeakIsFixed)
                    {
                        return;
                    }

                    onlyOnStartupUntilHttpClientMemoryLeakIsFixed = true;

                    await serverMonitorSemaphoreSlim.WaitAsync().ConfigureAwait(true);

                    try
                    {
                        await RefreshServerMonitorsAsync().ConfigureAwait(true);

                        var connectServers = serverMonitors.Where(
                            s => !s.IsConnected
                            && !s.IsConnecting 
                            && !string.IsNullOrWhiteSpace(s.Uri.ToString()) 
                            && s.Enabled).ToList();

                        if(connectServers.Any())
                        {
                            await Task.WhenAll(connectServers.Select(s => s.ConnectAsync(dispatcher)).ToList()).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnServerMonitorCacheNotification($"Observing Servers : {ex.Message}", ex);
                    }
                    finally
                    {
                        if (interval != serverConfiguration.ObserveServerInterval)
                        {
                            interval = serverConfiguration.ObserveServerInterval;
                        }

                        serverMonitorSemaphoreSlim.Release();
                    }
                });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch all exceptions and notify observers.")]
        private async Task RefreshServerMonitorsAsync()
        {
            try
            {
                var servers = await configurationServer.GetTradeServersAsync().ConfigureAwait(true);

                await dispatcher.InvokeAsync(async () =>
                {
                    var removeServers = serverMonitors.Where(sm => !servers.Any(s => s.Name == sm.Name)).ToList();
                    if (removeServers.Any())
                    {
                        await Task.WhenAll(removeServers.Select(s => s.DisposeAsync()).ToList()).ConfigureAwait(true);

                        foreach (var server in removeServers)
                        {
                            serverMonitorSubscriptions[server.Name].Dispose();
                            serverMonitorSubscriptions.Remove(server.Name);
                            serverMonitors.Remove(server);
                        }
                    }

                    static ServerMonitor updateServerMonitor(ServerMonitor sm, Core.Server.TradeServer s)
                    {
                        sm.Uri = s.Uri;
                        sm.MaxDegreeOfParallelism = s.MaxDegreeOfParallelism;
                        sm.Enabled = s.Enabled;
                        return sm;
                    };

                    (from sm in serverMonitors
                     join s in servers on sm.Name equals s.Name
                     select updateServerMonitor(sm, s)).ToList();

                    var newServers = servers.Where(s => !serverMonitors.Any(sm => sm.Name == s.Name)).ToList();

                    var newServerMonitors = newServers.Select(s => s.ToServerMonitor(httpClientManager.HttpClientInstance)).ToList();

                    foreach (var newServerMonitor in newServerMonitors)
                    {
                        serverMonitors.Add(newServerMonitor);
                        ObserveServerMonitor(newServerMonitor);
                    }
                });

                if (serverConfiguration == null)
                {
                    serverConfiguration = await configurationServer.GetServerConfiguration().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                OnServerMonitorCacheNotification($"Refreshing Servers : {ex.Message}", ex);
            }
        }

        private void OnServerMonitorCacheNotification(string message, Exception exception = null)
        {
            var serverMonitorCacheNotification = ServerMonitorCacheNotification;
            serverMonitorCacheNotification?.Invoke(this, new ServerMonitorCacheEventArgs { Value = this, Message = message, Exception = exception });
        }

        private void ObserveServerMonitor(ServerMonitor serverMonitor)
        {
            var serverMonitorObservable = Observable.FromEventPattern<ServerMonitorEventArgs>(
                eventHandler => serverMonitor.OnServerMonitorNotification += eventHandler,
                eventHandler => serverMonitor.OnServerMonitorNotification -= eventHandler)
                .Select(eventPattern => eventPattern.EventArgs);

            var serverMonitorSubscription = serverMonitorObservable.Subscribe(args =>
            {
                if (args.HasException)
                {
                    OnServerMonitorCacheNotification($"{args.Value.Name} : {args.Exception.Message}", args.Exception);
                }
                else
                {
                    OnServerMonitorCacheNotification($"{args.Message}");
                }
            });

            serverMonitorSubscriptions.Add(serverMonitor.Name, serverMonitorSubscription);
        }
    }
}
