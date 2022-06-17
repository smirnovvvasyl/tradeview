﻿//-----------------------------------------------------------------------
// <copyright file="IViewContext.cs" company="Development In Progress Ltd">
//     Copyright © 2012. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using DevelopmentInProgress.TradeView.Wpf.Host.Controller.Navigation;
using Prism.Regions;

namespace DevelopmentInProgress.TradeView.Wpf.Host.Controller.Context
{
    /// <summary>
    /// Interface for the <see cref="ViewContext"/> class
    /// which inherits abstract class <see cref="ContextBase"/>.
    /// </summary>
    public interface IViewContext : IContext
    {
        /// <summary>
        /// Gets an instance of <see cref="ModalNavigator"/> 
        /// enabling it to manage model pop-up windows.
        /// </summary>
        ModalNavigator ModalNavigator { get; }

        /// <summary>
        /// Gets an instance of <see cref="NavigationManager"/> enabling it to 
        /// manage model windows and to manage documents via prism navigation.
        /// </summary>
        NavigationManager NavigationManager { get; }

        /// <summary>
        /// Gets an instance of <see cref="IRegionManager"/>.
        /// </summary>
        IRegionManager RegionManager { get; }
    }
}
