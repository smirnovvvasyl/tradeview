﻿//-----------------------------------------------------------------------
// <copyright file="ModalViewBase.cs" company="Development In Progress Ltd">
//     Copyright © 2012. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using DevelopmentInProgress.TradeView.Wpf.Host.Controller.Context;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.ViewModel;
using System;

namespace DevelopmentInProgress.TradeView.Wpf.Host.Controller.View
{
    /// <summary>
    /// Base abstract class for the modal view. Inherits the <see cref="ViewBase"/> class.
    /// </summary>
    public abstract class ModalViewBase : ViewBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModalViewBase"/> class.
        /// </summary>
        /// <param name="viewContext"></param>
        protected ModalViewBase(IViewContext viewContext)
            : base(viewContext)
        {
        }

        /// <summary>
        /// Register the event handlers for showing message boxes and modal windows.
        /// </summary>
        /// <param name="modalViewModel">The modal view model raising the events.</param>
        public void RegisterDialogEventsHandlers(ModalViewModel modalViewModel)
        {
            if (modalViewModel == null)
            {
                throw new ArgumentNullException(nameof(modalViewModel));
            }

            modalViewModel.ShowMessageWindow += ShowMessageBox;
            modalViewModel.ShowModalWindow += ShowModalWindow;
        }
    }
}