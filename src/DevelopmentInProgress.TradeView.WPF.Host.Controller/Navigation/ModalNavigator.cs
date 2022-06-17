﻿//-----------------------------------------------------------------------
// <copyright file="ModalNavigator.cs" company="Development In Progress Ltd">
//     Copyright © 2012. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using DevelopmentInProgress.TradeView.Wpf.Controls.Messaging;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.View;
using DevelopmentInProgress.TradeView.Wpf.Host.Controller.ViewModel;
using System;
using System.Windows.Media.Imaging;
using Unity;

namespace DevelopmentInProgress.TradeView.Wpf.Host.Controller.Navigation
{
    /// <summary>
    /// The modal manager is responsible for interaction with 
    /// modal forms such as modal windows and message boxes.
    /// </summary>
    public class ModalNavigator
    {
        private readonly IUnityContainer container;

        /// <summary>
        /// Intitializes a new instance of the <see cref="ModalNavigator"/> class.
        /// </summary>
        /// <param name="container">Unity container.</param>
        public ModalNavigator(IUnityContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// Shows a message box and sets the message box result.
        /// </summary>
        /// <param name="messageBoxSettings">The message box settings.</param>
        public static void ShowMessageBox(MessageBoxSettings messageBoxSettings)
        {
            Dialog.ShowMessage(messageBoxSettings);
        }

        /// <summary>
        /// Shows a modal window and sets the resulting output in the 
        /// <see cref="ModalSettings"/> which is returned to the calling code.
        /// </summary>
        /// <param name="modalSettings">The <see cref="ModalSettings"/>.</param>
        public void ShowModal(ModalSettings modalSettings)
        {
            if(modalSettings == null)
            {
                throw new ArgumentNullException(nameof(modalSettings));
            }

            var viewType = Type.GetType(modalSettings.View);
            var resolvedView = container.Resolve(viewType, modalSettings.View);
            var view = (ModalViewBase)resolvedView;
            var viewModelType = Type.GetType(modalSettings.ViewModel);
            var viewModel = container.Resolve(viewModelType, modalSettings.ViewModel);
            ((ModalViewModel)viewModel).Publish(modalSettings.Parameters);
            view.RegisterDialogEventsHandlers((ModalViewModel)viewModel);
            view.DataContext = viewModel;
            var window = new ModalViewHost(view)
            {
                Icon = new BitmapImage(new Uri(@"pack://application:,,/Images/Origin.png", UriKind.RelativeOrAbsolute)),
                Title = modalSettings.Title ?? string.Empty,
                Height = modalSettings.Height,
                Width = modalSettings.Width
            };

            var result = window.ShowDialog();
            modalSettings.Result = result;
            modalSettings.Output = ((ModalViewModel)viewModel).Output;
        }

        /// <summary>
        /// Shows an error message in a dialog window.
        /// </summary>
        /// <param name="e">The exception to show.</param>
        public static void ShowError(Exception e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            ShowError(e.Message, e.StackTrace);
        }

        /// <summary>
        /// Shows an error message in a dialog window.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        /// <param name="stackTrace">The error stack trace.</param>
        public static void ShowError(string message, string stackTrace)
        {
            Dialog.ShowException(message, stackTrace);
        }
    }
}
