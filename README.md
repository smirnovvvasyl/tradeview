# TradeView

[![Build Status](https://ci.appveyor.com/api/projects/status/lsf9kuf5p93wvr0p/branch/master?svg=true)](https://ci.appveyor.com/project/grantcolley/tradeview/branch/master)

##### Technologies
* ###### .NET Core 3.1, .Net Standard 2.1, WPF, ASP.NET Core WebHost, WebSockets, Prism, Unity
#####  

A platform for trading crypto currencies and running crypto currency strategies including [TradeView](#tradeview-wpf-ui), which is a WPF UI built on the [Origin](https://github.com/grantcolley/origin) framework, and [TradeServer](#tradeserver-aspnetcore-webhost), a ASP.NET Core web host.

**TradeView** and **TradeServer** enables you to: 
* Connect to your exchage accounts, trade currencies, manage open orders and your asset balances
* Subscribe to live trade feed and order book for assets
* Create and run your own custom trading strategies
* Monitor running trading strategies
* Send real-time updates to strategy parameters for running strategies
* Easily extend the number of supported exchanges by creating wrappers for exchange api's that implement a common interface (*currently supports Binance and Kucoin exchanges*)

![Alt text](/README-images/tradeview.PNG?raw=true "Trade View")

#### Table of Contents
* [Getting Started](#getting-started)
  * [Setting up your dev environment](#setting-up-your-dev-environment)
  * [Start Trading](#start-trading)
  * [Run a Strategy](#run-a-strategy)  
  * [Update a Running Strategy](#update-a-running-strategy)    
  * [Create a Strategy](#create-a-strategy)
  * [Create a WPF Strategy Component](#create-a-wpf-strategy-component)
* [TradeView WPF UI](#tradeview-wpf-ui)
  * [Overview](#overview)
    * [Configuration Module](#configuration-module)
      * [Manage Accounts](#manage-accounts)
      * [Manage Strategies](#manage-strategies)
      * [Manage Servers](#manage-servers)
    * [Trading Module](#trading-module)    
    * [Strategies Module](#strategies-module)
    * [Dashboard Module](#dashboard-module)
      * [Monitoring Accounts](#monitoring-accounts)
      * [Monitoring Trade Servers](#monitoring-trade-servers)
* [TradeServer AspNetCore WebHost](#tradeserver-aspnetcore-webhost)
  * [The Console](#the-console)
  * [WebHost](#webhost)
  * [Startup](#startup)
     - [Request pipelines and Middleware](#request-pipelines-and-middleware)
     - [StrategyRunnerBackgroundService](#strategyrunnerbackgroundservice)
     - [StrategyNotificationHub](#strategynotificationhub)
  * [Batch Notifications](#batch-notifications)
     - [Batch Notification Types](#batch-notification-types)
* [Strategies](#strategies)
  * [Running a Strategy](#running-a-strategy)
     - [The Client Request](#the-client-request)
     - [The RunStrategyMiddleware](#the-runstrategymiddleware)
     - [The StrategyRunnerActionBlock](#the-strategyrunneractionblock)
     - [The StrategyRunner](#the-strategyrunner)
  * [Caching Running Strategies](#caching-running-strategies)
  * [Caching Running Strategies Subscriptions](#caching-running-strategies-subscriptions)
  * [Monitoring a Running Strategy](#monitoring-a-running-strategy)
     - [The Client Request to Monitor a Strategy](#the-client-request-to-monitor-a-strategy)
     - [The SocketMiddleware](#the-socketmiddleware)
  * [Updating Strategy Parameters](#updating-strategy-parameters)
     - [The Client Request to Update a Strategy](#the-client-request-to-update-a-strategy)
     - [The UpdateStrategyMiddleware](#the-updatestrategymiddleware)
  * [Stopping a Running Strategy](#stopping-a-running-strategy)
     - [The Client Request to Stop a Strategy](#the-client-request-to-stop-a-strategy)
     - [The StopStrategyMiddleware](#the-stopstrategymiddleware)
* [Extending TradeView](#extending-tradeview)
  * [Adding a new Exchange API](#adding-a-new-exchange-api)
  * [Persisting Configuration Data](#persisting-configuration-data)
  
# Getting Started

> **_TIP:_**
>> See [thread #88](https://github.com/grantcolley/tradeview/issues/88) for some additional advice about getting the demo strategy to run. 

## Setting up your dev environment
Download the source code. Open the solution file **TradeView.sln**, go to multiple start-up projects and select the following projects to start:
* DevelopmentInProgress.TradeView.Wpf.Host
* DevelopmentInProgress.TradeServer.Console

Compile and run the solution. This will start both the TradeView UI and the TradeServer WebHost, which runs as a console app.

## Start Trading
Select the [Trading](#trading-module) module in the navigational panel then click **Demo Account**. This will open the trading tab for the **Demo Account** and connect to its exchange, Binance, and subscribe to order book and trade feed for the default asset.

To trade you must create a new account. You can do this in the [Configuration](#configuration-module) module by selecting [Manage Accounts](#manage-accounts). Specify the exchange and provide an api key and api secret (and optionally a api pass phrase e.g. for Kucoin accounts). Also sepcify the default asset. Once the account has been created you can access it in the [Trading](#trading-module) module. You will see the account balances and real-time trade feed and order book for the default asset. You can start trading and receive real-time updates for your open orders.

## Run a Strategy  
Select the [Strategies](#strategies-module) module in the navigation panel and click the **Binance Moving Average - ETHBTC** demo strategy. This will open the strategy tab. From the drop down list in the top left corned select the server you want to run the strategy on. The default is *Trade Server*, which runs as a console app. Click on the **Run Strategy** button. This will upload the strategy to the *Trade Server* instance and start running it. In the **Trade Server** console window you will see logged in the console the request has been received to run the strategy. A webSocket connection is established between the **TradeView** and **TradeServer** and the **TradeServer** publishes notifications to the **TradeView** so the strategy can be monitored in real time.

*Note: the demo strategy only shows the real time trades with moving average and buy / sell indicators above and below the moving average.*

> **_TIP:_**
>> See [thread #88](https://github.com/grantcolley/tradeview/issues/88) for some additional advice about getting the demo strategy to run. 

See [Strategies](#strategies) for more details of how running a strategy works.

## Update a Running Strategy
Strategy parameters contain information the startegy uses to determine when to buy and sell assets. You can update the parameters of a running strategy in real-time. For example, in the UI the strategy parameters is displayed in JSON format and can be edited. Bump the sell indicator of the demo strategy from **0.00015** to **0.00115** and then click the **push** button. The updated strategy parameters is pushed to the strategy running on the server. You will notice in the trade chart the sell indicator line is adjusted accordingly.

See [Updating Strategy Parameters](#updating-strategy-parameters) for more details of how updating strategy parameters works.

## Create a Strategy
Creating a strategy involves creating a class library with a class that inherits [TradeStrategyBase](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/TradeStrategyBase.cs) which implements [ITradeStrategy](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/ITradeStrategy.cs).

You will also need to create a config entry for the strategy in the [Configuration](#configuration-module) module by selecting [Manage Strategies](#manage-strategies).

The best way to understand how to create a strategy is to look at the demo strategy [MovingAverageStrategy](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.Strategy.MovingAverage/MovingAverageStrategy.cs) and how it has been configured.

## Create a WPF Strategy Component
Creating a WPF strategy component involves creating a class library with a view model that inherits [StrategyDisplayViewModelBase](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Wpf.Common/ViewModel/StrategyDisplayViewModelBase.cs) and a corresponding view.

The best way to understand how to create a WPF strategy component is to look at the project [DevelopmentInProgress.Strategy.MovingAverage.Wpf](https://github.com/grantcolley/tradeview/tree/master/src/DevelopmentInProgress.Strategy.MovingAverage.Wpf) for an example of how the demo WPF strategy component has been created. 

You will also need to update the config entry for the strategy in the [Configuration](#configuration-module) module by selecting [Manage Strategies](#manage-strategies).

# TradeView WPF UI
## Overview
**TradeView** consists of modules, accessible from the navigation panel on the left, including [Configuration](#configuration-module), [Trading](#trading-module), [Strategies](#strategies-module) and the [Dashboard-module](#dashboard).
  
![Alt text](/README-images/navigationpanel.PNG?raw=true "Navigation Panel")

#### Configuration Module
The Configuration module is where configuration for trading accounts, running strategies and strategy servers is managed.

* [Manage Accounts](#manage-accounts)
* [Manage Strategies](#manage-strategies)
* [Manage Servers](#manage-servers)

###### Manage Accounts
Allows you to create and persist trading accounts including account name, exchange, api key and secret key. It also persists display preferences for the trading screen, such as favourite symbols, and default selected symbol.

![Alt text](/README-images/configuration_account.PNG?raw=true "Configure an Account")

###### Manage Strategies
Manage configuration for running a trading strategy and displaying a running strategy in realtime.
![Alt text](/README-images/configuration_strategy.PNG?raw=true "Configure a Strategy")

###### Manage Servers
Manage trade server details for servers that run trading strategies 

![Alt text](/README-images/configuration_server.PNG?raw=true "Configure a Server")

#### Trading Module
The Trading module shows a list of trading accounts in the navigation panel. Selecting an account will open a trading document in the main window for that account. From the trading document you can:
* see the account's balances
* view realtime pricing of favourite symbols
* select a symbol to subscribe to live orderbook and trade feed
* place buy and sell orders
* monitor and manage open orders in realtime 

![Alt text](/README-images/tradeview.PNG?raw=true "Trade View")

#### Strategies Module
Strategies are run on an instance of [TradeServer](#tradeserver-aspnetcore-webhost) and can be monitored by one or more users. It is possible to update a running strategy's parameters in realtime e.g. buy and sell triggers or suspend trading. See [Running a Strategy](#running-a-strategy).

![Alt text](/README-images/strategies.PNG?raw=true "Strategies")

#### Dashboard Module
* [Monitoring Accounts](#monitoring-accounts)
* [Monitoring Trade Servers](#monitoring-trade-servers)

###### Monitoring Accounts
You can view all accounts and open orders in realtime.

![Alt text](/README-images/dashboard_accounts.PNG?raw=true "Monitor Accounts in the Dashboard")

###### Monitoring Trade Servers
You can view all configured [TradeServer's](#tradeserver-aspnetcore-webhost) and whether they are active. An active [TradeServer](#tradeserver-aspnetcore-webhost) will show the strategies currently running on it, including each strategy's parameters and its active connections i.e. which users are monitoring the strategy. See [Running a Strategy](#running-a-strategy).

![Alt text](/README-images/dashboard_servers.PNG?raw=true "Monitor Trade Servers in the Dashboard")

# TradeServer AspNetCore WebHost
## The Console
The [console app](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.Console/Program.cs) takes three parameters:
- **s** = server name
- **u** = url of the webhost
- **p** = MaxDegreeOfParallelism for the dataflow StrategyRunnerActionBlock execution options

It creates and runs an instance of a WebHost, passing the parameters into it.

`dotnet DevelopmentInProgress.TradeServer.Console.dll --s=ServerName --u=http://+:5500 --p=5`

## WebHost

```C#
          var webHost = WebHost.CreateDefaultBuilder()
              .UseUrls(url)
              .UseStrategyRunnerStartup(args)
              .UseSerilog()
              .Build();
```

The WebHost's [UseStrategyRunnerStartup](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/WebHostExtensions.cs) extension method passes in the command line args to the WebHost and specifies the Startup class to use.

```C#
          public static class WebHostExtensions
          {
              public static IWebHostBuilder UseStrategyRunnerStartup(this IWebHostBuilder webHost, string[] args)
              {
                  return webHost.ConfigureAppConfiguration((hostingContext, config) =>
                  {
                      config.AddCommandLine(args);
                  }).UseStartup<Startup>();
              }
          }
```

## Startup
The [Startup](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Startup.cs) configures the request processing pipeline to branch the request path to the appropriate middleware and configures the services to be consumed via dependency injection. 

```C#
        public void Configure(IApplicationBuilder app)
        {
            app.UseDipSocket<NotificationHub>("/notificationhub");

            app.Map("/runstrategy", HandleRun);
            app.Map("/updatestrategy", HandleUpdate);
            app.Map("/stopstrategy", HandleStop);
            app.Map("/isstrategyrunning", HandleIsStrategyRunning);
            app.Map("/ping", HandlePing);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var server = new Server();
            server.Started = DateTime.Now;
            server.StartedBy = Environment.UserName;
            server.Name = Configuration["s"].ToString();
            server.Url = Configuration["u"].ToString();
            if(Convert.ToInt32(Configuration["p"]) > 0)
            {
                server.MaxDegreeOfParallelism = Convert.ToInt32(Configuration["p"]);
            }

            services.AddSingleton<IServer>(server);
            services.AddSingleton<IStrategyRunnerActionBlock, StrategyRunnerActionBlock>();
            services.AddSingleton<IStrategyRunner, StrategyRunner>();
            services.AddSingleton<INotificationPublisherContext, NotificationPublisherContext>();
            services.AddSingleton<INotificationPublisher, NotificationPublisher>();
            services.AddSingleton<IBatchNotificationFactory<StrategyNotification>, StrategyBatchNotificationFactory>();
            services.AddSingleton<IExchangeApiFactory, ExchangeApiFactory>();
            services.AddSingleton<IExchangeService, ExchangeService>();
            services.AddSingleton<IExchangeSubscriptionsCacheFactory, ExchangeSubscriptionsCacheFactory>();
            services.AddSingleton<ISubscriptionsCacheManager, SubscriptionsCacheManager>();
            services.AddSingleton<ITradeStrategyCacheManager, TradeStrategyCacheManager>();

            services.AddHostedService<StrategyRunnerBackgroundService>();

            services.AddDipSocket<NotificationHub>();
        }
```

#### Request pipelines and Middleware
The following table shows the middleware each request path is mapped to. 
|Request Path|Maps to Middleware|Description|
|------------|------------------|-----------|
|`http://localhost:5500/runstrategy`|[RunStrategyMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/RunStrategyMiddleware.cs)|Request to run a strategy|
|`http://localhost:5500/stopstrategy`|[StopStrategyMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/StopStrategyMiddleware.cs)|Stop a running strategy|
|`http://localhost:5500/updatestrategy`|[UpdateStrategyMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/UpdateStrategyMiddleware.cs)|Update a running strategy's parameters|
|`http://localhost:5500/isstrategyrunning`|[IsStrategyRunningMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/IsStrategyRunningMiddleware.cs)|Check if a strategy is running|
|`http://localhost:5500/ping`|[PingMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/PingMiddleware.cs)|Check if the trade server is running|
|`http://localhost:5500/notificationhub`|[SocketMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.Socket.Extensions/SocketMiddleware.cs)|A websocket connection request|

#### StrategyRunnerBackgroundService
The Startup class adds a long running hosted service [StrategyRunnerBackgroundService](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/HostedService/StrategyRunnerBackgroundService.cs) which inherits the BackgroundService. It is a long running background task for running trade strategies that have been posted to the trade servers runstrategy request pipeline. It contains a reference to the singleton [StrategyRunnerActionBlock](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/HostedService/StrategyRunnerActionBlock.cs) which has an ActionBlock dataflow that invokes an ActionBlock<StrategyRunnerActionBlockInput> delegate for each request to run a trade strategy.

```C#
          strategyRunnerActionBlock.ActionBlock = new ActionBlock<StrategyRunnerActionBlockInput>(async actionBlockInput =>
          {
               await actionBlockInput.StrategyRunner.RunAsync(actionBlockInput.Strategy,
                                                              actionBlockInput.DownloadsPath,
                                                              actionBlockInput.CancellationToken)
                                                              .ConfigureAwait(false);
          }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = server.MaxDegreeOfParallelism });
```

#### StrategyNotificationHub
The application uses [Socket](https://github.com/grantcolley/tradeview/tree/master/src/DevelopmentInProgress.Socket), based on [DipSocket](https://github.com/grantcolley/dipsocket), a lightweight publisher / subscriber implementation using WebSockets, for sending and receiving notifications to and from clients and servers. The [StrategyNotificationHub](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Notification/Strategy/StrategyNotificationHub.cs) inherits the abstract class [SocketServer](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.Socket/Server/SocketServer.cs) to manage client connections and channels. A client e.g. a running instance of [TradeView](#tradeview-wpf-ui), establishes a connection to the server with the purpose of running or monitoring a strategy on it. The strategy registers a Socket channel to which multiple client connections can subscribe. The strategy broadcasts notifications (e.g. live trade feed, buy and sell orders etc.) to the client connections. The [StrategyNotificationHub](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Notification/Strategy/StrategyNotificationHub.cs) overrides the OnClientConnectAsync and ReceiveAsync methods.

```C#
          public async override Task OnClientConnectAsync(WebSocket websocket, string clientId, string strategyName)
          {
                var connection = await base.AddWebSocketAsync(websocket).ConfigureAwait(false);

                SubscribeToChannel(strategyName, websocket);

                var connectionInfo = connection.GetConnectionInfo();

                var json = JsonConvert.SerializeObject(connectionInfo);

                var message = new Message { MethodName = "OnConnected", SenderConnectionId = "StrategyRunner", Data = json };

                await SendMessageAsync(websocket, message).ConfigureAwait(false);
          }
        
          public async override Task ReceiveAsync(WebSocket webSocket, Message message)
          {
                switch (message.MessageType)
                {
                    case MessageType.UnsubscribeFromChannel:
                        UnsubscribeFromChannel(message.Data, webSocket);
                        break;
                }
          }
```

## Batch Notifications
Batch notifiers inherit abstract class [BatchNotification<T>](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Notification/BatchNotification.cs) which uses a BlockingCollection<T> for adding notifications to a queue while sending them on on a background thread.

#### Batch Notification Types
```C#
    public enum BatchNotificationType
    {
        StrategyRunnerLogger,
        StrategyAccountInfoPublisher,
        StrategyCustomNotificationPublisher,
        StrategyNotificationPublisher,
        StrategyOrderBookPublisher,
        StrategyTradePublisher,
        StrategyStatisticsPublisher,
        StrategyCandlesticksPublisher
    }
```

The [StrategyBatchNotificationFactory](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Notification/Strategy/StrategyBatchNotificationFactory.cs) creates instances of batch notifiers. Batch notifiers use the [StrategyNotificationPublisher](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Notification/Strategy/StrategyNotificationPublisher.cs) to publish notifications (trade, order book, account notifications etc) to client connections.

# Strategies
## Running a Strategy
#### The Client Request
The clients loads the serialised [Strategy](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/Strategy.cs) and strategy assemblies into a MultipartFormDataContent and post a request to the server.

```C#
            var client = new HttpClient();
            var multipartFormDataContent = new MultipartFormDataContent();

            var jsonContent = JsonConvert.SerializeObject(strategy);

            multipartFormDataContent.Add(new StringContent(jsonContent, Encoding.UTF8, "application/json"), "strategy");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var fileStream = File.OpenRead(file);
                using (var br = new BinaryReader(fileStream))
                {
                    var byteArrayContent = new ByteArrayContent(br.ReadBytes((int)fileStream.Length));
                    multipartFormDataContent.Add(byteArrayContent, fileInfo.Name, fileInfo.FullName);
                }
            }

            Task<HttpResponseMessage> response = await client.PostAsync("http://localhost:5500/runstrategy", multipartFormDataContent);
```
#### The RunStrategyMiddleware
The [RunStrategyMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/RunStrategyMiddleware.cs) processes the request on the server. It deserialises the strategy and downloads the strategy assemblies into a sub directory under the working directory of the application. The running of the strategy is then passed to the [StrategyRunnerActionBlock](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/HostedService/StrategyRunnerActionBlock.cs).

```C#
                var json = context.Request.Form["strategy"];

                var strategy = JsonConvert.DeserializeObject<Strategy>(json);

                var downloadsPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads", Guid.NewGuid().ToString());

                if (context.Request.HasFormContentType)
                {
                    var form = context.Request.Form;

                    var downloads = from f
                                    in form.Files
                                    select Download(f, downloadsPath);

                    await Task.WhenAll(downloads.ToArray());
                }

                var strategyRunnerActionBlockInput = new StrategyRunnerActionBlockInput
                {
                    StrategyRunner = strategyRunner,
                    Strategy = strategy,
                    DownloadsPath = downloadsPath
                };

                await strategyRunnerActionBlock.RunStrategyAsync(strategyRunnerActionBlockInput).ConfigureAwait(false);
```

#### The StrategyRunnerActionBlock
The [StrategyRunnerActionBlock](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/HostedService/StrategyRunnerActionBlock.cs), hosted in the [StrategyRunnerBackgroundService](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/HostedService/StrategyRunnerBackgroundService.cs), has an ActionBlock dataflow that invokes an ActionBlock<StrategyRunnerActionBlockInput> delegate for each request to run a trade strategy by calling the [StrategyRunner.RunAsync](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/StrategyRunner.cs) method, passing in the strategy and location of the strategy assemblies to run.

```C#
          strategyRunnerActionBlock.ActionBlock = new ActionBlock<StrategyRunnerActionBlockInput>(async actionBlockInput =>
          {
               await actionBlockInput.StrategyRunner.RunAsync(actionBlockInput.Strategy,
                                                              actionBlockInput.DownloadsPath,
                                                              actionBlockInput.CancellationToken)
                                                              .ConfigureAwait(false);
          }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = server.MaxDegreeOfParallelism });
```

#### The StrategyRunner
A strategy must implement [ITradeStrategy](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/ITradeStrategy.cs). 
The [StrategyRunner](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/StrategyRunner.cs) will load the strategy's assembly and all its dependencies into memory and create an instance of the strategy. It will subscribe to strategy events to handle notifications from the strategy to the client connection i.e. the [TradeView](#tradeview-wpf-ui) UI. The strategy is added to [TradeStrategyCacheManager](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Cache/TradeStrategy/TradeStrategyCacheManager.cs) and the [SubscriptionsCacheManager](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Cache/Subscriptions/SubscriptionsCacheManager.cs) subscribes to the strategy's feeds i.e. trade, orderbook, account updates etc. Finally, the [ITradeStrategy.RunAsync](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/ITradeStrategy.cs) method is called to run the strategy.

```C#
        internal async Task<Strategy> RunStrategyAsync(Strategy strategy, string localPath)
        {
            ITradeStrategy tradeStrategy = null;

            try
            {
                var dependencies = GetAssemblies(localPath);

                var assemblyLoader = new AssemblyLoader(localPath, dependencies);
                var assembly = assemblyLoader.LoadFromMemoryStream(Path.Combine(localPath, strategy.TargetAssembly));
                var type = assembly.GetType(strategy.TargetType);
                dynamic obj = Activator.CreateInstance(type);

                tradeStrategy = (ITradeStrategy)obj;

                tradeStrategy.StrategyNotificationEvent += StrategyNotificationEvent;
                tradeStrategy.StrategyAccountInfoEvent += StrategyAccountInfoEvent;
                tradeStrategy.StrategyOrderBookEvent += StrategyOrderBookEvent;
                tradeStrategy.StrategyTradeEvent += StrategyTradeEvent;
                tradeStrategy.StrategyStatisticsEvent += StrategyStatisticsEvent;
                tradeStrategy.StrategyCandlesticksEvent += StrategyCandlesticksEvent;
                tradeStrategy.StrategyCustomNotificationEvent += StrategyCustomNotificationEvent;

                strategy.Status = StrategyStatus.Running;

                if(tradeStrategyCacheManager.TryAddTradeStrategy(strategy.Name, tradeStrategy))
                {
                    await subscriptionsCacheManager.Subscribe(strategy, tradeStrategy).ConfigureAwait(false);
                    
                    var result = await tradeStrategy.RunAsync(strategy, cancellationToken).ConfigureAwait(false);

                    if(!tradeStrategyCacheManager.TryRemoveTradeStrategy(strategy.Name, out ITradeStrategy ts))
                    {
                        Notify(NotificationLevel.Error, $"Failed to remove {strategy.Name} from the cache manager.");
                    }
                }
                else
                {
                    Notify(NotificationLevel.Error, $"Failed to add {strategy.Name} to the cache manager.");
                }
            }
            finally
            {
                if(tradeStrategy != null)
                {
                    subscriptionsCacheManager.Unsubscribe(strategy, tradeStrategy);

                    tradeStrategy.StrategyNotificationEvent -= StrategyNotificationEvent;
                    tradeStrategy.StrategyAccountInfoEvent -= StrategyAccountInfoEvent;
                    tradeStrategy.StrategyOrderBookEvent -= StrategyOrderBookEvent;
                    tradeStrategy.StrategyTradeEvent -= StrategyTradeEvent;
                    tradeStrategy.StrategyCustomNotificationEvent -= StrategyCustomNotificationEvent;
                }
            }

            return strategy;
        }
```

## Caching Running Strategies
The [TradeStrategyCacheManager](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Cache/TradeStrategy/TradeStrategyCacheManager.cs) caches running instances of strategies and is used to [check if a strategy is running](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/IsStrategyRunningMiddleware.cs), [update a running strategy's parameters](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/UpdateStrategyMiddleware.cs), and [stopping a strategy](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/StopStrategyMiddleware.cs).

## Caching Running Strategies Subscriptions
Strategies can [subscribe](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/Subscribes.cs) to feeds for one or more symbols across multiple exhanges. 

```C#
    [Flags]
    public enum Subscribe
    {
        None = 0,
        AccountInfo = 1,
        Trades = 2,
        OrderBook = 4,
        Candlesticks = 8
    }
```

The [SubscriptionsCacheManager](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Cache/Subscriptions/SubscriptionsCacheManager.cs) manages symbols subscriptions across all exchanges using the [ExchangeSubscriptionsCacheFactory](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Cache/Subscriptions/ExchangeSubscriptionsCacheFactory.cs) which provides an instance of the [ExchangeSubscriptionsCache](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Cache/Subscriptions/ExchangeSubscriptionsCache.cs) for each exchange.

The [IExchangeSubscriptionsCache](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Cache/Subscriptions/IExchangeSubscriptionsCache.cs) uses a dictionary for caching symbol subscriptions for the exchange it was created.

```C#
    public interface IExchangeSubscriptionsCache : IDisposable
    {
        bool HasSubscriptions { get; }
        ConcurrentDictionary<string, ISubscriptionCache> Caches { get; }
        Task Subscribe(string strategyName, List<StrategySubscription> strategySubscription, ITradeStrategy tradeStrategy);
        void Unsubscribe(string strategyName, List<StrategySubscription> strategySubscription, ITradeStrategy tradeStrategy);
    }
``` 

## Monitoring a Running Strategy
The application uses [Socket](https://github.com/grantcolley/tradeview/tree/master/src/DevelopmentInProgress.Socket), based on [DipSocket](https://github.com/grantcolley/dipsocket), a lightweight publisher / subscriber implementation using WebSockets, for sending and receiving notifications to and from clients and servers.

#### The Client Request to Monitor a Strategy
The [SocketClient's](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.Socket/Client/SocketClient.cs) `StartAsync` method opens WebSocket connection with the [SocketServer](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.Socket/Server/SocketServer.cs). The `On` method registers an Action to be invoked when receiving a message from the server.

```C#
            socketClient = new DipSocketClient($"{Strategy.StrategyServerUrl}/notificationhub", strategyAssemblyManager.Id);

            socketClient.On("Connected", message =>
            {
                ViewModelContext.UiDispatcher.Invoke(() =>
                {
                    NotificationsAdd(message);
                });
            });

            socketClient.On("Notification", async (message) =>
            {
                await ViewModelContext.UiDispatcher.Invoke(async () =>
                {
                    await OnStrategyNotificationAsync(message);
                });
            });

            socketClient.On("Trade", (message) =>
            {
                ViewModelContext.UiDispatcher.Invoke(async () =>
                {
                    await OnTradeNotificationAsync(message);
                });
            });

            socketClient.On("OrderBook", (message) =>
            {
                ViewModelContext.UiDispatcher.Invoke(async () =>
                {
                    await OnOrderBookNotificationAsync(message);
                });
            });

            socketClient.On("AccountInfo", (message) =>
            {
                ViewModelContext.UiDispatcher.Invoke(() =>
                {
                    OnAccountNotification(message);
                });
            });

            socketClient.On("Candlesticks", (message) =>
            {
                ViewModelContext.UiDispatcher.Invoke(async () =>
                {
                    await OnCandlesticksNotificationAsync(message);
                });
            });

            socketClient.Closed += async (sender, args) =>
            {
                await ViewModelContext.UiDispatcher.Invoke(async () =>
                {
                    NotificationsAdd(message);

                    await socketClient.DisposeAsync();
                });
            };

            socketClient.Error += async (sender, args) => 
            {
                await ViewModelContext.UiDispatcher.Invoke(async () =>
                {
                    NotificationsAdd(message);
                    
                    await socketClient.DisposeAsync()
                });
            };
            
            await socketClient.StartAsync(strategy.Name);
```

#### The SocketMiddleware
The [SocketMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.Socket.Extensions/SocketMiddleware.cs) processes the request on the server. The [StrategyNotificationHub](#strategynotificationhub) manages client connections.

```C#
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            var clientId = context.Request.Query["clientId"];
            var data = context.Request.Query["data"];

            await dipSocketServer.OnClientConnectAsync(webSocket, clientId, data);

            await Receive(webSocket);

```

```C#
        private async Task Receive(WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024 * 4];
                var messageBuilder = new StringBuilder();

                while (webSocket.State.Equals(WebSocketState.Open))
                {
                    WebSocketReceiveResult webSocketReceiveResult;

                    messageBuilder.Clear();

                    do
                    {
                        webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (webSocketReceiveResult.MessageType.Equals(WebSocketMessageType.Close))
                        {
                            await dipSocketServer.OnClientDisonnectAsync(webSocket);
                            continue;
                        }

                        if (webSocketReceiveResult.MessageType.Equals(WebSocketMessageType.Text))
                        {
                            messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count));
                            continue;
                        }
                    }
                    while (!webSocketReceiveResult.EndOfMessage);

                    if (messageBuilder.Length > 0)
                    {
                        var json = messageBuilder.ToString();

                        var message = JsonConvert.DeserializeObject<Message>(json);

                        await dipSocketServer.ReceiveAsync(webSocket, message);
                    }
                }
            }
            finally
            {
                webSocket?.Dispose();
            }
        }
```

## Updating Strategy Parameters
#### The Client Request to Update a Strategy
The [StrategyRunnerClient](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/StrategyRunnerClient.cs) posts a message to the strategy server to update a running strategy's parameters.

```C#
           var strategyParametersJson = JsonConvert.SerializeObject(strategyParameters, Formatting.Indented);
           
           var strategyRunnerClient = new StrategyRunnerClient();

           var response = await strategyRunnerClient.PostAsync($"{Strategy.StrategyServerUrl}/updatestrategy", strategyParametersJson);
```

#### The UpdateStrategyMiddleware
The [UpdateStrategyMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/UpdateStrategyMiddleware.cs) processes the request on the server.

```C#
           var json = context.Request.Form["strategyparameters"];

           var strategyParameters = JsonConvert.DeserializeObject<StrategyParameters>(json);

           if(tradeStrategyCacheManager.TryGetTradeStrategy(strategyParameters.StrategyName, out ITradeStrategy tradeStrategy))
           {
               await tradeStrategy.TryUpdateStrategyAsync(json);
           }
```

## Stopping a Running Strategy
#### The Client Request to Stop a Strategy
The [StrategyRunnerClient](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/TradeStrategy/StrategyRunnerClient.cs) posts a message to the strategy server to stop a running strategy.

```C#
           var strategyParametersJson = JsonConvert.SerializeObject(strategyParameters, Formatting.Indented);
           
           var strategyRunnerClient = new Strategy.StrategyRunnerClient();

           var response = await strategyRunnerClient.PostAsync($"{Strategy.StrategyServerUrl}/stopstrategy", strategyParametersJson);
```

#### The StopStrategyMiddleware
The [StopStrategyMiddleware](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeServer.StrategyExecution.WebHost/Web/Middleware/StopStrategyMiddleware.cs) processes the request on the server.

```C#
           var json = context.Request.Form["strategyparameters"];

           var strategyParameters = JsonConvert.DeserializeObject<StrategyParameters>(json);

           if (tradeStrategyCacheManager.TryGetTradeStrategy(strategyParameters.StrategyName, out ITradeStrategy tradeStrategy))
           {
               await tradeStrategy.TryStopStrategy(json);
           }
```

# Extending TradeView
## Adding a new Exchange API
**TradeView** is intended to trade against multiple exchanges and the following api's are currently supported:
* [Binance](https://github.com/sonvister/Binance)
* [Kucoin.Net](https://github.com/JKorf/Kucoin.Net)

To add a new api create a new .NET Standard project for the API wrapper and create a class that implements [IExchangeApi](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/Interfaces/IExchangeApi.cs).
For example see [BinanceExchangeApi](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Api.Binance/BinanceExchangeApi.cs). 

```C#
namespace DevelopmentInProgress.TradeView.Api.Binance
{
    public class BinanceExchangeApi : IExchangeApi
    {
        public async Task<Order> PlaceOrder(User user, ClientOrder clientOrder)
        {
            var binanceApi = new BinanceApi();
            using (var apiUser = new BinanceApiUser(user.ApiKey, user.ApiSecret))
            {
                var order = OrderHelper.GetOrder(apiUser, clientOrder);
                var result = await binanceApi.PlaceAsync(order).ConfigureAwait(false);
                return NewOrder(user, result);
            }
        }

        public async Task<string> CancelOrderAsync(User user, string symbol, string orderId, string newClientOrderId = null, long recWindow = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            var binanceApi = new BinanceApi();
            var id = Convert.ToInt64(orderId);
            using (var apiUser = new BinanceApiUser(user.ApiKey, user.ApiSecret))
            {
                var result = await binanceApi.CancelOrderAsync(apiUser, symbol, id, newClientOrderId, recWindow, cancellationToken).ConfigureAwait(false);
                return result;
            }
        }
```

Next, add the exchange to the [Exchange](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Core/Enums/Exchange.cs) enum.

```C#
    public enum Exchange
    {
        Unknown,
        Binance,
        Kucoin,
        Test
    }
```

Finally, return an instance of the new exchange from the [ExchangeApiFactory](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Service/ExchangeApiFactory.cs).

```C#
    public class ExchangeApiFactory : IExchangeApiFactory
    {
        public IExchangeApi GetExchangeApi(Exchange exchange)
        {
            switch(exchange)
            {
                case Exchange.Binance:
                    return new BinanceExchangeApi();
                case Exchange.Kucoin:
                    return new KucoinExchangeApi();
                default:
                    throw new NotImplementedException();
            }
        }

        public Dictionary<Exchange, IExchangeApi> GetExchanges()
        {
            var exchanges = new Dictionary<Exchange, IExchangeApi>();
            exchanges.Add(Exchange.Binance, GetExchangeApi(Exchange.Binance));
            exchanges.Add(Exchange.Kucoin, GetExchangeApi(Exchange.Kucoin));
            return exchanges;
        }
    }
```

## Persisting Configuration Data
Data can be persisted to any data source by creating a library with classes that implement the interfaces in [DevelopmentInProgress.TradeView.Data](https://github.com/grantcolley/tradeview/tree/master/src/DevelopmentInProgress.TradeView.Data).
* [ITradeViewConfigurationAccounts](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Data/ITradeViewConfigurationAccounts.cs)
* [ITradeViewConfigurationServer](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Data/ITradeViewConfigurationServer.cs)
* [ITradeViewConfigurationStrategy](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Data/ITradeViewConfigurationStrategy.cs)

And register the classes in the TradeView's [App.xaml.cs](https://github.com/grantcolley/tradeview/blob/master/src/DevelopmentInProgress.TradeView.Wpf.Host/App.xaml.cs) RegisterTypes method.
```C#
            containerRegistry.Register<ITradeViewConfigurationAccounts, TradeViewConfigurationAccountsFile>();
            containerRegistry.Register<ITradeViewConfigurationStrategy, TradeViewConfigurationStrategyFile>();
            containerRegistry.Register<ITradeViewConfigurationServer, TradeViewConfigurationServerFile>();
```
