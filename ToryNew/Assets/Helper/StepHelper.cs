using CommunityToolkit.Labs.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToryNew.Assets.Helper {
    public class StepHelper {
        
        // loop through every Control / TextBlock and Disable/Enable it accordingly, if is StackPanel then do the same method inside of the StackPanel
        public static void DisableOrEnable(StackPanel panel, bool enableOrDisable) {
            if (panel == null) return;
            var controls = panel.Children.Where(s => { return s is Control; });
            var textBlocks = panel.Children.Where(s => { return s is TextBlock; });
            var stackPanels = panel.Children.Where(s => { return s is StackPanel; });

            foreach (var c in controls) {
                if (c is SettingsCard) {
                    SettingsCard s = (SettingsCard)c;
                    DisableOrEnable(s.Content as StackPanel, enableOrDisable);
                }
                ((Control)c).IsEnabled = enableOrDisable;
            }
            foreach (var t in textBlocks) {
                SolidColorBrush color = new SolidColorBrush();
                color.Color = enableOrDisable ? Colors.White : Colors.Gray;
                ((TextBlock)t).Foreground = color;
            }
            foreach (var s in stackPanels) {
                DisableOrEnable((StackPanel)s, enableOrDisable);
            }
        }
    }
}
