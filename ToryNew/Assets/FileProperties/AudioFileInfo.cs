using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Storage.Provider;

namespace ToryNew.Assets.FileProperties
{
    public class AudioFileInfo : INotifyPropertyChanged
    {
        private uint imageSize;
        public AudioFileInfo(MusicProperties properties,
            StorageFile audioFile,
            string name,
            string type)
        {
            AudioProperties = properties;
            AudioName = name;
            AudioFileType = type;
            AudioFile = audioFile;
            imageSize = 600;
        }

        public StorageFile AudioFile { get; }

        public MusicProperties AudioProperties { get; }

        public async Task<BitmapImage> GetImageSourceAsync()
        {
            using IRandomAccessStream fileStream = await AudioFile.OpenReadAsync();

            // Create a bitmap to be the image source.
            BitmapImage bitmapImage = new();
            bitmapImage.SetSource(fileStream);

            return bitmapImage;
        }

        public async Task<BitmapImage> GetImageThumbnailAsync()
        {
            StorageItemThumbnail thumbnail;
            try {
                thumbnail = await AudioFile.GetThumbnailAsync(ThumbnailMode.MusicView, imageSize);
            } catch(Exception e) {
                thumbnail = null;
            }
            // Create a bitmap to be the image source.
            var bitmapImage = new BitmapImage();
            if (thumbnail == null)
            {
                StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
                StorageFolder picturesFolder = await appInstalledFolder.GetFolderAsync("Assets\\Samples");
                StorageFile file = await StorageFile.GetFileFromPathAsync($"{picturesFolder.Path}\\placeholderAudio.png");
                bitmapImage.SetSource(await file.GetThumbnailAsync(ThumbnailMode.PicturesView));
            }
            else {
                bitmapImage.SetSource(thumbnail);
                thumbnail.Dispose();
            }

            return bitmapImage;
        }

        public string AudioName { get; }

        public string AudioFileType { get; }

        public string AudioTitle {
            get => string.IsNullOrEmpty(AudioProperties.Title) ? AudioName : AudioProperties.Title;
            set
            {
                if (AudioProperties.Title != value)
                {
                    AudioProperties.Title = value;
                    _ = AudioProperties.SavePropertiesAsync();
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
