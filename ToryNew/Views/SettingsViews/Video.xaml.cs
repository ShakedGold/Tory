// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Labs.WinUI;
using FFmpeg.NET.Enums;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Tory.Views;
using Tory.Views.ConversionViews;
using ToryNew.Assets.AppSettings;
using ToryNew.Views.SettingsViews;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ToryNew.Views.SettingConversionViews {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class Video : Page {

        public Video() {
            this.InitializeComponent();

            MainWindow.navigationView.IsBackEnabled = true;
            MainWindow.navigationView.BackRequested += NavBar_BackRequested;
            MainWindow.navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;

            switch(AppSettings.CodecSelected.Value) {
                case VideoCodec.Default:
                    CodecComboBoxFFmpeg.SelectedIndex = 0;
                    break;
                case VideoCodec.nvenc_h264:
                    CodecComboBoxFFmpeg.SelectedIndex = 1;
                    break;
            }
        }

        private void NavBar_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) {
            Settings.NavigateToView(typeof(General));
            MainWindow.navigationView.IsBackEnabled = false;
            MainWindow.navigationView.BackRequested -= NavBar_BackRequested;
            MainWindow.navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }
        private void CodecComboBoxSelection_Changed(object sender, SelectionChangedEventArgs e) {
            VideoCodec codec = VideoCodec.Default;
            int selection = ((ComboBox)sender).SelectedIndex;
            switch(selection) {
                case 1:
                    codec = VideoCodec.nvenc_h264;
                    break;
                case 0:
                    codec = VideoCodec.Default;
                    break;
            }
            AppSettings.CodecSelected.Update(codec);
        }
    }
}
