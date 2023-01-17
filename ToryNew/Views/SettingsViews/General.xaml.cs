// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Labs.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Reflection;
using ToryNew.Assets.AppSettings;
using Tory.Views.ConversionViews;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ToryNew.Views.SettingsViews {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class General : Page {
        public General() {
            this.InitializeComponent();

            ThemeComboBox.SelectedIndex = Math.Clamp((int)AppSettings.SelectedTheme.Value, 0, 2);
            ConversionComboBox.SelectedIndex = (int)AppSettings.DefaultConversionMethod.Value;
        }
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            AppSettings.SelectedTheme.Update((ThemeSelection)((ComboBox)sender).SelectedIndex);
        }

        private void ConversionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            AppSettings.DefaultConversionMethod.Update((ConversionMethod)((ComboBox)sender).SelectedIndex);
        }

        private void ConversionSetting_Clicked(object sender, RoutedEventArgs e) {
            string tag = ((SettingsCard)sender).Tag.ToString();
            var view = Assembly.GetExecutingAssembly().GetType($"ToryNew.Views.SettingConversionViews.{tag}");

            Settings.NavigateToView(view);
        }
    }
}
