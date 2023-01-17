// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Labs.WinUI;
using CommunityToolkit.WinUI.UI;
using FFmpeg.NET;
using FFmpeg.NET.Enums;
using FFmpeg.NET.Events;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToryNew;
using ToryNew.Assets.FileProperties;
using ToryNew.UserControls;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using WinRT.Interop;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tory.Views.ConversionViews {

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Video : Page {
        public int currentStep; // Keeps track of the current step
        public int convertedFiles; // number of converted files
        private IReadOnlyList<StorageFile> files; // list of files
        private ProgressBar progressBar; // the progress bar that will keep track of how many files have been converted (global to change its value outside of an event)
        private TextBlock convertedInfo;
        private DispatcherQueue dispatcherQueue;
        private CancellationTokenSource cancelSource;
        public ObservableCollection<VideoFileInfo> Images { get; } = new ObservableCollection<VideoFileInfo>();

        public Video() {
            this.InitializeComponent();
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            currentStep = 1;

            //get and set the formats
            FormatSelectionCombo.Items.Clear();
            FormatSelectionCombo.ItemsSource = Enum.GetValues(typeof(VideoFormat)).Cast<VideoFormat>();
            FormatSelectionCombo.SelectedIndex = 0;

            //Set the defalt preserve file toggle button
            PreserveFileSettingsToggle.IsOn = true;

            // disable all of the steps other than the first (2 - 4) to prevent changing the settings and starting the conversion before there are any files
            DisableOrEnable(Step2, false);
            DisableOrEnable(Step3, false);
            DisableOrEnable(Step4, false);
        }

        // The event for the button that will pick the files in step 1
        public async void BrowseFiles(object sender, RoutedEventArgs e) {
            // Create the file picker
            var filePicker = new FileOpenPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = WindowNative.GetWindowHandle(App.Window);

            // Associate the HWND with the file picker
            InitializeWithWindow.Initialize(filePicker, hwnd);

            //file picker settings
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            foreach (var en in Enum.GetValues(typeof(VideoFormat)).Cast<VideoFormat>()) {
                filePicker.FileTypeFilter.Add("." + en.ToString());
            }


            files = await filePicker.PickMultipleFilesAsync();
            if (files.Count == 0) return;

            //Move to Step 2&3
            toStep2and3();
        }

        // The event for the button that will pick the folder in step 1
        public async void BrowseFolder(object sender, RoutedEventArgs e) {
            var folderPicker = new FolderPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = WindowNative.GetWindowHandle(App.Window);

            // Associate the HWND with the file picker
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            //folder Picker settings
            folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            folderPicker.FileTypeFilter.Add("*");

            //pick a folder
            var folder = await folderPicker.PickSingleFolderAsync();


            //if picked a folder continue if not dont go to the next page
            if (folder == null) return;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

            //pick all the files in a directory
            files = await folder.GetFilesAsync();
            files.Where(s => { return Enum.GetValues(typeof(VideoFormat)).Cast<string>().ToList().Contains(s.FileType); });

            //Move to Step 2&3
            toStep2and3();
        }

        // after picking the folder in which the files will be save then open step 4
        public async void SaveFolder(object sender, RoutedEventArgs e) {
            var folderPicker = new FolderPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = WindowNative.GetWindowHandle(App.Window);

            // Associate the HWND with the file picker
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            //folder Picker settings
            folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            folderPicker.FileTypeFilter.Add("*");

            //pick a folder
            var folder = await folderPicker.PickSingleFolderAsync();

            //if picked a folder continue if not dont go to the next page
            if (folder == null) return;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            saveFolderTextBlock.Text = folder.Path;

            if (currentStep >= 4) return;
            currentStep = 4;
            DisableOrEnable(Step4, true);
        }

        // loop through every Control / TextBlock and Disable/Enable it accordingly, if is StackPanel then do the same method inside of the StackPanel
        private void DisableOrEnable(Step panel, bool enableOrDisable) {
            panel.IsEnabled = enableOrDisable;
        }
        private void DisableOrEnable(StackPanel panel, bool enableOrDisable) {
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

        //Start converting the files display the loading message
        private async void StartConversionButton(object sender, RoutedEventArgs e) {
            //the inside display
            StackPanel stackPanel = new StackPanel();
            stackPanel.Spacing = 10;

            progressBar = new ProgressBar();
            convertedInfo = new TextBlock();
            convertedFiles = 0;

            //Display the number of converted files
            convertedInfo.Text = "Converted Files: " + convertedFiles + " Out of " + files.Count;

            // initialize the progress bar
            progressBar.Minimum = 0;
            progressBar.Maximum = files.Count;
            progressBar.Value = 0;

            stackPanel.Children.Add(convertedInfo);
            stackPanel.Children.Add(progressBar);

            //create a content dialog with the stackpanel
            var cd = new ContentDialog {
                Title = "Converting Videos",
                Content = stackPanel,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Done"
            };
            cd.PrimaryButtonClick += PressedDone;
            cd.CloseButtonClick += PressedCancel;
            cd.XamlRoot = this.Content.XamlRoot;

            //Start Conversion
            StartConversion();
            //Show the dialog message with the info of the converted files
            var result = await cd.ShowAsync();
        }

        public async void StartConversion() {
            ComboBoxItem cbi;
            //Format Text Grab
            if (FormatSelectionCombo.SelectedItem == null) return;
            VideoFormat format = (VideoFormat)FormatSelectionCombo.SelectedItem;

            //Quality Grab
            if (QualitySelectionCombo.SelectedItem == null) return;
            cbi = (ComboBoxItem)QualitySelectionCombo.SelectedItem;
            string quality = cbi.Content.ToString().Split("p")[0];

            //Bitrate Slider
            int bitrateValue = (int)SliderBitrate.Value;

            //FPS Value
            if (FPSCombo.SelectedItem == null) return;
            cbi = (ComboBoxItem)FPSCombo.SelectedItem;
            int fps = int.Parse(cbi.Content.ToString().ToLower());

            //check if the preserve settings toggle is on if so choose the copy codec for faster conversion
            //set default codec, future setting
            var codec = VideoCodec.Default;
            switch (format) {
                case VideoFormat.WEBP:
                    codec = VideoCodec.libwebp;
                    break;
                case VideoFormat.WEBM:
                    break;
                default:
                    codec = PreserveFileSettingsToggle.IsOn ? VideoCodec.copy : codec;
                    break;
            }

            //looping through all the videos and using ffmpeg to convert them
            foreach (StorageFile file in files) {
                var inputFile = new InputFile(file.Path);
                var outputFilePath = $"{saveFolderTextBlock.Text}\\{file.DisplayName}.{format.ToString().ToLower()}";
                var outputFile = new OutputFile(outputFilePath);
                cancelSource = new CancellationTokenSource();

                var ffmpeg = new Engine("E:\\Tory Project\\ffmpeg\\ffmpeg.exe");
                ffmpeg.Complete += OnComplete;
                ffmpeg.Progress += ConversionProgress;
                ffmpeg.Error += OnError;

                ConversionOptions conversionOptions;

                if (format == VideoFormat.GIF) {
                    if (fps == 60) fps = 50;
                    else fps = 33;
                    conversionOptions = new ConversionOptions {
                        ExtraArguments = $"-vf \"fps={fps},split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\" -loop 0 -y",
                        VideoSize = (VideoSize)Enum.Parse(typeof(VideoSize), $"Hd{quality}")
                    };
                } else {
                    conversionOptions = new ConversionOptions {
                        VideoCodec = codec,
                        VideoFps = fps,
                        VideoBitRate = bitrateValue * 1000,
                        VideoSize = (VideoSize)Enum.Parse(typeof(VideoSize), $"Hd{quality}")
                    };
                }

                //in try and catch for the cancel button, catching the cancel exception to reset the nessecary variables and quit the conversion
                try {
                    await ffmpeg.ConvertAsync(inputFile, outputFile, conversionOptions, cancelSource.Token);
                } catch (TaskCanceledException e) {
                    cancelSource.Dispose();
                    convertedFiles = 0;
                    progressBar.Value = 0;
                    break;
                }
            }
        }

        private void OnError(object sender, ConversionErrorEventArgs e) {
            throw new Exception(e.Exception.Message);
        }

        private void ConversionProgress(object sender, ConversionProgressEventArgs e) {
            DispatcherQueue.TryEnqueue(() => {
                progressBar.Value = (e.ProcessedDuration / e.TotalDuration) + convertedFiles;
            });
        }

        private void OnComplete(object sender, ConversionCompleteEventArgs e) {
            DispatcherQueue.TryEnqueue(() => {
                convertedInfo.Text = "Converted Files: " + convertedFiles + " Out of " + files.Count;
            });
            convertedFiles++;
        }

        //What to do when presing done, if the files are converted then allow the exit else don't.
        private void PressedDone(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            if (convertedFiles == files.Count) {
                sender.Hide();
            } else args.Cancel = true;
        }

        private void PressedCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            cancelSource.Cancel();
        }

        //some settings are not compatible with certain formats so disable and enable them accordingly
        private void FormatSelectionChanged(object sender, RoutedEventArgs e) {
            ComboBox comboBox = (ComboBox)sender;
            VideoFormat selection = (VideoFormat)comboBox.SelectedItem;
            
            if (selection == VideoFormat.GIF) {
                PreserveFileSetting.IsEnabled = false;
                PreserveFileSettingsToggle.IsOn = false;
                DisableOrEnable(NotPreservedSettingsPanel, false);
            } else if (selection == VideoFormat.WEBM || selection == VideoFormat.WEBP) {
                if (PreserveFileSettingsToggle == null) return;
                PreserveFileSetting.IsEnabled = false;
                PreserveFileSettingsToggle.IsOn = false;
                DisableOrEnable(NotPreservedSettingsPanel, true);
            } else {
                PreserveFileSetting.IsEnabled = currentStep != 1;
                DisableOrEnable(NotPreservedSettingsPanel, !PreserveFileSettingsToggle.IsOn);
            }
        }

        private void PreserveFileSettingsToggle_Toggled(object sender, RoutedEventArgs e) {
            //toggle between enabled settings and disabled
            DisableOrEnable(NotPreservedSettingsPanel, !((ToggleSwitch)sender).IsOn);
        }

        private void toStep2and3() {
            //count the files
            VideoNumberText.Text = files.Count.ToString();

            //show the video selection grid
            showVideoSelection();
            MediaPlayer.AreTransportControlsEnabled = true;
            VideoSelectionAdaptiveGrid.SelectedIndex = 0;

            //set the current step
            if (currentStep >= 3) return;
            currentStep = 3;

            //enable the settings
            DisableOrEnable(Step2, true);
            DisableOrEnable(Step3, true);
            PreserveFileSetting.IsEnabled = true;
            DisableOrEnable(NotPreservedSettingsPanel, false);
        }

        private async void showVideoSelection() {
            Images.Clear();
            foreach (StorageFile file in files) {
                Images.Add(await LoadImageInfo(file));
            }
            VideoSelectionAdaptiveGrid.ItemsSource = Images;
        }

        public async static Task<VideoFileInfo> LoadImageInfo(StorageFile file) {
            var properties = await file.Properties.GetVideoPropertiesAsync();
            VideoFileInfo info = new(properties,
                                     file, file.DisplayName, file.DisplayType);

            return info;
        }

        private void VideoSelectionView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args) {
            if (args.InRecycleQueue) {
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = templateRoot.FindName("ItemImage") as Microsoft.UI.Xaml.Controls.Image;
                image.Source = null;
            }

            if (args.Phase == 0) {
                args.RegisterUpdateCallback(ShowImage);
                args.Handled = true;
            }
        }

        private async void ShowImage(ListViewBase sender, ContainerContentChangingEventArgs args) {
            if (args.Phase == 1) {
                // It's phase 1, so show this item's image.
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = templateRoot.FindName("ItemImage") as Microsoft.UI.Xaml.Controls.Image;
                var item = args.Item as VideoFileInfo;
                image.Source = await item.GetImageThumbnailAsync();
            }
        }

        private void VideoSelectionAdaptiveGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count < 1) return;
            VideoFileInfo videoFileInfo = e.AddedItems[0] as VideoFileInfo;
            if (videoFileInfo != null) {
                MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(videoFileInfo.VideoFile.Path));
            }
        }
    }
}
