// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tory.Views;
using ToryNew.Views.SettingsViews;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ToryNew.Views.SettingConversionViews {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Audio : Page {
        public Audio() {
            this.InitializeComponent();

            MainWindow.navigationView.IsBackEnabled = true;
            MainWindow.navigationView.BackRequested += NavBar_BackRequested;
            MainWindow.navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
        }
        private void NavBar_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) {
            Settings.NavigateToView(typeof(General));
            MainWindow.navigationView.IsBackEnabled = false;
            MainWindow.navigationView.BackRequested -= NavBar_BackRequested;
            MainWindow.navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }
    }
}
