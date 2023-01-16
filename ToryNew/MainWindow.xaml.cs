// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using ToryNew.Assets;
using ToryNew.Assets.AppSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ToryNew {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow : Window {
        private NavigationViewItem _lastItem;
        public static NavigationView navigationView;

        public MainWindow() {
            this.InitializeComponent();


            navigationView = NavBar;
            NavigateToView(AppSettings.DefaultConversionMethod.ToString());
        }

        //changing the view whenevr the selection in NavBar is pressed
        private void NavBar_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            var item = args.InvokedItemContainer as NavigationViewItem;
            if (item == null || item == _lastItem) return;

            var clickedView = item.Tag.ToString();
            if (!NavigateToView(clickedView)) return;
            _lastItem = item;
        }

        //Getting the view itself
        private bool NavigateToView(string clickedView) {
            var view = Assembly.GetExecutingAssembly().GetType($"Tory.Views.{clickedView}");
            if (string.IsNullOrWhiteSpace(clickedView)) return false;
            if (view == null) return false;
            ContentFrame.Navigate(view, null, new EntranceNavigationTransitionInfo());
            return true;
        }

        //loads the Video view when the programs first runs and removes the back button
        private void NavBar_Loaded(object sender, RoutedEventArgs e) {
            NavBar.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            NavBar.SelectedItem = NavBar.MenuItems.Where(s => {
                if (s is NavigationViewItem)
                    return (string)((NavigationViewItem)s).Tag == AppSettings.DefaultConversionMethod.ToString();
                else return false;
            }).ToArray()[0];
            NavigateToView(AppSettings.DefaultConversionMethod.ToString());
        }
    }
}
