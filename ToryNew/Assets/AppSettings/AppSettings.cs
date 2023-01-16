using FFmpeg.NET.Enums;
using System.Collections.Generic;
using Windows.Storage;

namespace ToryNew.Assets.AppSettings {
    public static class AppSettings {
        private static ThemeSelection systemTheme;

        public static Setting<ThemeSelection> SelectedTheme;
        public static Setting<ConversionMethod> DefaultConversionMethod;
        public static Setting<VideoCodec> CodecSelected;

        public static void Init() {
            //Initializing the Selected Theme to be correct, wether its the System Theme or the selected one.
            SelectedTheme = new Setting<ThemeSelection>(ThemeSelection.DefaultLight, "selectedTheme");
            DefaultConversionMethod = new Setting<ConversionMethod>(ConversionMethod.Video, "defaultConversionView");
            CodecSelected = new Setting<VideoCodec>(VideoCodec.Default, "codecSelectedFFmpeg");

            //set system default or selected theme
            systemTheme = (ThemeSelection)(int)App.Current.RequestedTheme + 2;
            if (SelectedTheme.Value != systemTheme && (int)SelectedTheme.Value >= 2) {
                SelectedTheme.Value = systemTheme;
            }
            else {
                SelectedTheme.Value = (ThemeSelection)ApplicationData.Current.LocalSettings.Values[SelectedTheme.Name];
            }
        }
    }
}
