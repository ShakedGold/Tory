// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Labs.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using ToryNew;
using ToryNew.Assets.AppSettings;
using ToryNew.Views.SettingsViews;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tory.Views.ConversionViews {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class Settings : Page {
        public static Frame ContentFrame;

        public Settings() {
            this.InitializeComponent();
            ContentFrame = new Frame();
            ScrollView.Content = ContentFrame;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e) {
            ContentFrame.Navigate(typeof(General), null, new EntranceNavigationTransitionInfo());
        }

        public static void NavigateToView(Type type) {
            ContentFrame.Navigate(type, null);
        }

        private void ScrollView_Unloaded(object sender, RoutedEventArgs e) {
            MainWindow.navigationView.IsBackEnabled = false;
            MainWindow.navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }
    }
}
