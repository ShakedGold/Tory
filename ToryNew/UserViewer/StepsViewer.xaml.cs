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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ToryNew.UserViewer {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StepsViewer : Page, INotifyPropertyChanged {
        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();

        private int currentStep;

        public event PropertyChangedEventHandler PropertyChanged;

        public int CurrentStep {
            get {
                return currentStep;
            }
            set {
                currentStep = value;
                SetValue(CurrentStepProperty, value);
            }
        }

        public static readonly DependencyProperty CurrentStepProperty = DependencyProperty.Register(
            "CurrentStep",
            typeof(int),
            typeof(StepsViewer),
            new PropertyMetadata(null)
        );

        public StepsViewer() {
            this.InitializeComponent();
            this.DataContext = this;
        }
    }
}
