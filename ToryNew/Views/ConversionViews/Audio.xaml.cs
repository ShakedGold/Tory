// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using FFmpeg.NET.Enums;
using FFmpeg.NET.Events;
using FFmpeg.NET;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using ToryNew;
using ToryNew.Assets.FileProperties;
using ToryNew.Assets.Helper;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tory.Views.ConversionViews {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Audio : Page {
        private IReadOnlyList<StorageFile> files; // list of files
        public ObservableCollection<AudioFileInfo> Images { get; } = new ObservableCollection<AudioFileInfo>();
        private ProgressBar progressBar;
        private TextBlock convertedInfo;
        public int convertedFiles;
        private CancellationTokenSource cancelSource;

        public Audio() {
            this.InitializeComponent();
            this.DataContext = this;

            StepsViewer.Init(15);

            FormatSelectionCombo.Items.Clear();
            FormatSelectionCombo.ItemsSource = Enum.GetValues(typeof(AudioFormat)).Cast<AudioFormat>();
            FormatSelectionCombo.SelectedIndex = 0;

            //Set the defalt preserve file toggle button
            PreserveFileSettingsToggle.IsOn = true;
        }

        private void PreserveFileSettingsToggle_Toggled(object sender, RoutedEventArgs e) {
            StepHelper.DisableOrEnable(NotPreservedSettingsPanel, !((ToggleSwitch)sender).IsOn);
        }

        // The event for the button that will pick the files in step 1
        public async void BrowseFiles(object sender, RoutedEventArgs e) {
            files = await BrowseHelper.BrowseFiles<AudioFormat>();

            toStep2and3();
        }

        // The event for the button that will pick the folder in step 1
        public async void BrowseFolder(object sender, RoutedEventArgs e) {
            files = await BrowseHelper.BrowseFolder<AudioFormat>();

            toStep2and3();
        }

        private async void SaveFolder(object sender, RoutedEventArgs e) {
            saveFolderTextBlock.Text = await BrowseHelper.SaveFolder<AudioFormat>();
            StepsViewer.ToStep(4);
        }

        private void toStep2and3() {
            if (files == null) return;
            //count the files
            AudioNumberText.Text = files.Count.ToString();

            //show the video selection grid
            showVideoSelection();
            MediaPlayer.AreTransportControlsEnabled = true;
            AudioSelectionAdaptiveGrid.SelectedIndex = 0;

            //set the current step
            StepsViewer.ToStep(3);
            PreserveFileSetting.IsEnabled = true;
        }

        private async void showVideoSelection() {
            Images.Clear();
            foreach (StorageFile file in files) {
                Images.Add(await LoadImageInfo(file));
            }
            AudioSelectionAdaptiveGrid.ItemsSource = Images;
        }
        public async static Task<AudioFileInfo> LoadImageInfo(StorageFile file) {
            var properties = await file.Properties.GetMusicPropertiesAsync();
            AudioFileInfo info = new(properties,
                                     file, file.DisplayName, file.DisplayType);

            return info;
        }

        private void AudioSelectionAdaptiveGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count < 1) return;
            AudioFileInfo audioFileInfo = e.AddedItems[0] as AudioFileInfo;
            if (audioFileInfo != null) {
                MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(audioFileInfo.AudioFile.Path));
            }
        }

        private void AudioSelectionAdaptiveGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args) {
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
                var item = args.Item as AudioFileInfo;
                image.Source = await item.GetImageThumbnailAsync();
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
                Title = "Converting Audio Files",
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
            AudioFormat format = (AudioFormat)FormatSelectionCombo.SelectedItem;

            //Bitrate Grab
            var bitrate = SliderBitrate.Value;

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
                conversionOptions = new ConversionOptions {
                    AudioBitRate = (int)bitrate,
                    AudioChanel = 
                };

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
    }
}
