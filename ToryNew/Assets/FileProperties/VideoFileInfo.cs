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
    public class VideoFileInfo : INotifyPropertyChanged
    {
        private uint imageSize;
        public VideoFileInfo(VideoProperties properties,
            StorageFile imageFile,
            string name,
            string type)
        {
            VideoProperties = properties;
            VideoName = name;
            VideoFileType = type;
            VideoFile = imageFile;
            imageSize = 600;
        }

        public StorageFile VideoFile { get; }

        public VideoProperties VideoProperties { get; }

        public async Task<BitmapImage> GetImageSourceAsync()
        {
            using IRandomAccessStream fileStream = await VideoFile.OpenReadAsync();

            // Create a bitmap to be the image source.
            BitmapImage bitmapImage = new();
            bitmapImage.SetSource(fileStream);

            return bitmapImage;
        }

        public async Task<BitmapImage> GetImageThumbnailAsync()
        {
            StorageItemThumbnail thumbnail =
                await VideoFile.GetThumbnailAsync(ThumbnailMode.VideosView, imageSize);
            while (thumbnail == null || imageSize > 100)
            {
                imageSize -= 50;
                thumbnail = await VideoFile.GetThumbnailAsync(ThumbnailMode.VideosView, imageSize);
            }
            // Create a bitmap to be the image source.
            var bitmapImage = new BitmapImage();
            if (thumbnail == null)
            {
                StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
                StorageFolder picturesFolder = await appInstalledFolder.GetFolderAsync("Assets\\Samples");
                StorageFile file = await StorageFile.GetFileFromPathAsync($"{picturesFolder.Path}\\image1.jpg");
                bitmapImage.SetSource(await file.GetThumbnailAsync(ThumbnailMode.PicturesView));
            }
            else bitmapImage.SetSource(thumbnail);
            thumbnail.Dispose();

            return bitmapImage;
        }

        public string VideoName { get; }

        public string VideoFileType { get; }

        public string VideoDimensions => $"{VideoProperties.Width} x {VideoProperties.Height}";

        public string VideoTitle
        {
            get => string.IsNullOrEmpty(VideoProperties.Title) ? VideoName : VideoProperties.Title;
            set
            {
                if (VideoProperties.Title != value)
                {
                    VideoProperties.Title = value;
                    _ = VideoProperties.SavePropertiesAsync();
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
