﻿using DevelopmentInProgress.TradeView.Wpf.Common.Helpers;
using DevelopmentInProgress.TradeView.Wpf.Common.Model;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;

namespace DevelopmentInProgress.TradeView.Wpf.Strategies.Utility
{
    public class StrategyAssemblyManager : IStrategyAssemblyManager
    {
        private readonly IHelperFactoryContainer iHelperFactoryContainer;
        private AssemblyLoader assemblyLoader;
        private bool disposed;

        public StrategyAssemblyManager(IHelperFactoryContainer iHelperFactoryContainer)
        {
            this.iHelperFactoryContainer = iHelperFactoryContainer;

            Files = new List<string>();
        }

        public string Id { get; private set; }
        public string StrategyDirectory { get; private set; }
        public List<string> Files { get; private set; }
        public object StrategyDisplayView { get; private set; }
        public object StrategyDisplayViewModel { get; private set; }

        public void Activate(Strategy strategy, Dispatcher UiDispatcher, ILoggerFacade Logger)
        {
            if(strategy == null)
            {
                throw new ArgumentNullException(nameof(strategy));
            }

            Id = Environment.UserName;

            var displayAssemblies = strategy.DisplayDependencies.Select(a => a.File).ToList();

            Download(displayAssemblies);

            assemblyLoader = new AssemblyLoader(StrategyDirectory, Files);
            var assembly = assemblyLoader.LoadFromMemoryStream(Path.Combine(StrategyDirectory, strategy.DisplayAssembly.DisplayName));
            var viewModel = assembly.GetType(strategy.DisplayViewModelType);
            StrategyDisplayViewModel = Activator.CreateInstance(viewModel, new object[] { strategy, iHelperFactoryContainer, UiDispatcher, Logger });

            //var asm = Assembly.LoadFile(Path.Combine(StrategyDirectory, strategy.DisplayAssembly.DisplayName));

            //var viewModel = asm.GetType(strategy.DisplayViewModelType);

            //StrategyDisplayViewModel = Activator.CreateInstance(viewModel, 
            //    new object[] { strategy, iHelperFactoryContainer, UiDispatcher, Logger });

            var view = assembly.GetType(strategy.DisplayViewType);
            StrategyDisplayView = Activator.CreateInstance(view, new object[] { StrategyDisplayViewModel });
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (disposed)
            {
                return;
            }

            if (isDisposing)
            {
                if (StrategyDisplayViewModel != null)
                {
                    if (StrategyDisplayViewModel is IDisposable disposeableViewModel)
                    {
                        disposeableViewModel.Dispose();
                    }
                }

                if (StrategyDisplayView != null)
                {
                    if (StrategyDisplayView is IDisposable disposeableView)
                    {
                        disposeableView.Dispose();
                    }
                }

                disposed = true;
            }
        }

        private void Download(IEnumerable<string> files)
        {
            StrategyDirectory = Path.Combine(Directory.GetCurrentDirectory(), "strategies", $"{Id}_{Guid.NewGuid()}");

            if (!Directory.Exists(StrategyDirectory))
            {
                Directory.CreateDirectory(StrategyDirectory);
            }

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var assembly = Path.Combine(StrategyDirectory, fileInfo.Name);
                File.Copy(file, assembly, true);
                var name = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.'));
                Files.Add(name);                
            }
        }
    }
}