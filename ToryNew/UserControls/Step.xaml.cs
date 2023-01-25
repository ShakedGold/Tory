// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Labs.WinUI;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ToryNew.UserControls {
    public sealed partial class Step : UserControl {
        public int StepNumber { get; set; }
        public int Spacing { get; set; }
        public string Header { get; set; }
        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();

        public int currentStep { get; set; }
        public bool IsCurrentStep {
            get {
                return StepNumber <= currentStep;
            }
        }
        public Step() {
            this.InitializeComponent();
            this.DataContext = this;

            Debug.WriteLine("Spacing: " + Spacing);
        }

        public static void SetCurrentStep(ObservableCollection<Step> steps, int currStep) {
            foreach (var step in steps) {
                step.currentStep = currStep;
                step.StepControl.IsEnabled = step.IsCurrentStep;
            }
        }

        public static void SetStepSpacing(ObservableCollection<Step> steps, int spacing) {
            foreach (var step in steps) {
                step.Spacing = spacing;
            }
        }
    }
}
