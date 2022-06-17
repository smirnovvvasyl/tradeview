﻿//-----------------------------------------------------------------------
// <copyright file="ViewBase.cs" company="Development In Progress Ltd">
//     Copyright © 2012. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using DevelopmentInProgress.TradeView.Wpf.Controls.Messaging;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.Context;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.Navigation;
using Prism.Logging;
using System;
using System.Windows.Controls;

namespace DevelopmentInProgress.TradeView.Wpf.Host.Controller.View
{
    /// <summary>
    /// Base abstract class to be inherited by views.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "IDE0060:Remove unused parameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters")]
    public abstract class ViewBase : UserControl
    {
        protected IViewContext ViewContext { get; }
        protected ILoggerFacade Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewBase"/> class.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        protected ViewBase(IViewContext viewContext)
        {
            ViewContext = viewContext ?? throw new ArgumentNullException(nameof(viewContext));
            Logger = ViewContext.Logger;
        }


        /// <summary>
        /// Handles the ShowMessageBox event raised by the view model.
        /// </summary>
        /// <param name="sender">The view model.</param>
        /// <param name="e">Message box settings.</param>
        protected static void ShowMessageBox(object sender, MessageBoxSettings e)
        {
            ModalNavigator.ShowMessageBox(e);
        }



        /// <summary>
        /// Handles the ShowModalWindow event raised by the view model.
        /// </summary>
        /// <param name="sender">The view model.</param>
        /// <param name="e">Modal settings.</param>
        protected void ShowModalWindow(object sender, ModalSettings e)
        {
            ViewContext.ModalNavigator.ShowModal(e);
        }

        /// <summary>
        /// Handles the Publish event raised by the view model to open a new document.
        /// </summary>
        /// <param name="sender">The view model.</param>
        /// <param name="e">Navigation settings.</param>
        protected void Publish(object sender, NavigationSettings e)
        {
            ViewContext.NavigationManager.NavigateDocumentRegion(e);
        }

        /// <summary>
        /// Handles the NavigateTarget event raised by the view model to navigate to an open document.
        /// </summary>
        /// <param name="sender">The view model.</param>
        /// <param name="e">Navigation target.</param>
        protected void NavigateTarget(object sender, NavigationTarget e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            ViewContext.NavigationManager.NavigateDocumentRegion(e.NavigationId);
        }
    }
}
