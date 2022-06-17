﻿//-----------------------------------------------------------------------
// <copyright file="NavigationManager.cs" company="Development In Progress Ltd">
//     Copyright © 2012. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using DevelopmentInProgress.TradeView.Wpf.Host.Controller.View;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.ViewModel;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DevelopmentInProgress.TradeView.Wpf.Host.Controller.Navigation
{
    /// <summary>
    /// Manages navigating views to regions using Prism. 
    /// </summary>
    public class NavigationManager
    {
        private readonly object lockNavigationList;
        private readonly IRegionManager regionManager;
        private readonly Dictionary<string, NavigationSettings> navigationSettingsList;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationManager"/> class.
        /// </summary>
        /// <param name="regionManager">The Prism region manager.</param>
        public NavigationManager(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            navigationSettingsList = new Dictionary<string, NavigationSettings>();
            lockNavigationList = new object();
        }

        /// <summary>
        /// Build up a Uri string based on the <see cref="NavigationSettings"/> argument,
        /// assign it a navigation id and store as a key value pair in the navigation settings list. 
        /// Then navigate to that view using the navigation id.
        /// </summary>
        /// <param name="navigationSettings">
        /// <see cref="NavigationSettings"/> contains information about the target view 
        /// such as the view type, view title, parameters and navigation history.
        /// </param>
        public void NavigateDocumentRegion(NavigationSettings navigationSettings)
        {
            if(navigationSettings == null)
            {
                throw new ArgumentNullException(nameof(navigationSettings));
            }

            if (String.IsNullOrEmpty(navigationSettings.View))
            {
                throw new ArgumentNullException($"{nameof(navigationSettings)}.View");
            }

            var query = new NavigationParameters
            {
                { "Title", navigationSettings.Title ?? navigationSettings.View },
                { "Navigation", navigationSettings.NavigationHistory ?? String.Empty }
            };

            string partialUri = navigationSettings.View + query.ToString();
            navigationSettings.PartialQuery = partialUri;
            var navigationSettingsClone = (NavigationSettings)navigationSettings.Clone();
            string navigationId = String.Empty;
            lock (lockNavigationList)
            {
                var existingNavigationSetting = navigationSettingsList.Values.FirstOrDefault(
                    ns => ns.PartialQuery.Equals(partialUri, StringComparison.Ordinal) 
                        && (ns.Data == null || ns.Data.Equals(navigationSettings.Data)));
                if (existingNavigationSetting != null)
                {
                    navigationId = existingNavigationSetting.NavigationId;
                }
                else
                {
                    navigationId = GetNewNavigationSettingsListKey();
                    query.Add("NavigationId", navigationId);
                    var viewUri = navigationSettings.View + query.ToString();
                    navigationSettingsClone.NavigationId = navigationId;
                    navigationSettingsClone.ViewQuery = viewUri;
                    navigationSettingsList.Add(navigationId, navigationSettingsClone);
                }
            }

            NavigateDocumentRegion(navigationId); 
        }

        /// <summary>
        /// Return the next available key for the NavigationSettingsList 
        /// dictionary by getting the maximum key value and incrementing it by one.
        /// </summary>
        /// <returns>The next available key.</returns>
        private string GetNewNavigationSettingsListKey()
        {
            int maxKey = 0;
            foreach (string key in navigationSettingsList.Keys)
            {
                if (Int32.TryParse(key, out int iKey))
                {
                    if (iKey > maxKey)
                    {
                        maxKey = iKey;
                    }
                }
            }

            maxKey++;
            return maxKey.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Navigate to the DocumentRegion (document tab) passing in the view Uri.
        /// The view is obtained from the navigation list using the navigation id.
        /// </summary>
        /// <param name="navigationId">The navigation id of the Uri to navigate to.</param>
        public void NavigateDocumentRegion(string navigationId)
        {
            if (navigationSettingsList.TryGetValue(navigationId, out NavigationSettings navigationSettings))
            {
                NavigateRegion(navigationSettings.ViewQuery, "DocumentRegion");
                return;
            }

            var message = $"The navigation list does not contain a Uri for navigation id {navigationId}.";
            throw new Exception(message);
        }

        /// <summary>
        /// Uses Prism's region manager to navigate to the specified view at the specified region.
        /// </summary>
        /// <param name="view">The view to navigate to.</param>
        /// <param name="regionName">The specified region.</param>
        public void NavigateRegion(string view, string regionName)
        {
            regionManager.RequestNavigate(regionName,
                new Uri(view, UriKind.Relative),
                NavigationCompleted);
        }

        /// <summary>
        /// Removes the navigation settings from the navigation 
        /// settings list of the specified navigation id.
        /// </summary>
        /// <param name="navigationId">The navigation id of the navigation settings to remove.</param>
        public void CloseDocument(string navigationId)
        {
            if (String.IsNullOrEmpty(navigationId))
            {
                return;
            }

            lock (lockNavigationList)
            {
                if (navigationSettingsList.ContainsKey(navigationId))
                {
                    navigationSettingsList.Remove(navigationId);
                }
            }
        }

        /// <summary>
        /// Gets the view model for the given navigation id.
        /// </summary>
        /// <param name="navigationId">Identifies which view model to get.</param>
        /// <returns>The <see cref="DocumentViewModel"/> for the specified navigation id.</returns>
        public DocumentViewModel GetViewModel(string navigationId)
        {
            var navigationSettings = navigationSettingsList.FirstOrDefault(n => n.Value.NavigationId.Equals(navigationId, StringComparison.Ordinal));
            return navigationSettings.Value.DocumentView.ViewModel;
        }

        /// <summary>
        /// Gets all the view models for the specified module.
        /// </summary>
        /// <param name="moduleName">The module to which the view models belong.</param>
        /// <returns>A list of <see cref="DocumentViewModel"/> for the specified module.</returns>
        public List<DocumentViewModel> GetViewModels(string moduleName)
        {
            var documentViewModels = (from views
                                in navigationSettingsList
                                where views.Value.DocumentView != null
                                && views.Value.DocumentView.ModuleName.Equals(moduleName, StringComparison.Ordinal)
                                select views.Value.DocumentView.ViewModel).ToList();
            return documentViewModels;
        }

        /// <summary>
        /// Gets all view models for all modules.
        /// </summary>
        /// <returns>A list of <see cref="DocumentViewModel"/> for all modules.</returns>
        public List<DocumentViewModel> GetAllViewModels()
        {
            var documentViewModels = from views
                                in navigationSettingsList
                                select views.Value.DocumentView.ViewModel;
            return documentViewModels.ToList();
        }

        /// <summary>
        /// The navigation callback gets the view and stores a reference to it in the
        /// navigation settings. It also gets the data paremeter and passes it to the
        /// view model's by calling the Publish method.
        /// </summary>
        /// <param name="navigationResult">The navigation result.</param>
        private void NavigationCompleted(NavigationResult navigationResult)
        {
            if (navigationResult.Context.NavigationService.Region.Name.Equals("DocumentRegion", StringComparison.Ordinal))
            {
                if (navigationResult.Result.HasValue
                    && !navigationResult.Result.Value)
                {
                    // Navigation has been cancelled.
                    return;
                }

                var query = navigationResult.Context.Parameters;
                var navigationId = query["NavigationId"].ToString();

                if (navigationSettingsList.TryGetValue(navigationId, out NavigationSettings navigationSettings))
                {
                    object data = navigationSettings.Data;
                    var view = navigationResult.Context.NavigationService.Region.Views.FirstOrDefault(
                        v => (((DocumentViewBase)v).ViewModel.NavigationId.Equals(navigationId, StringComparison.Ordinal)));
                    var documentView = (DocumentViewBase)view;
                    navigationSettings.DocumentView = documentView;
                    documentView.ViewModel.PublishData(data);
                    return;
                }

                var message = $"The navigation list does not contain a Uri for navigation id {navigationId}.";
                throw new Exception(message);
            }
        }
    }
}
