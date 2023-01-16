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
    public class ImageFileInfo : INotifyPropertyChanged
    {
        private uint imageSize;
        public ImageFileInfo(ImageProperties properties,
            StorageFile imageFile,
            string name,
            string type)
        {
            ImageProperties = properties;
            ImageName = name;
            ImageFileType = type;
            ImageFile = imageFile;
            var random = new Random();
            imageSize = 600;
        }

        public StorageFile ImageFile { get; }

        public ImageProperties ImageProperties { get; }

        public async Task<BitmapImage> GetImageSourceAsync()
        {
            using IRandomAccessStream fileStream = await ImageFile.OpenReadAsync();

            // Create a bitmap to be the image source.
            BitmapImage bitmapImage = new();
            bitmapImage.SetSource(fileStream);

            return bitmapImage;
        }

        public async Task<BitmapImage> GetImageThumbnailAsync()
        {
            StorageItemThumbnail thumbnail =
                await ImageFile.GetThumbnailAsync(ThumbnailMode.VideosView, imageSize);
            while (thumbnail == null || imageSize > 100)
            {
                imageSize -= 50;
                thumbnail = await ImageFile.GetThumbnailAsync(ThumbnailMode.VideosView, imageSize);
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

        public string ImageName { get; }

        public string ImageFileType { get; }

        public string ImageDimensions => $"{ImageProperties.Width} x {ImageProperties.Height}";

        public string ImageTitle
        {
            get => string.IsNullOrEmpty(ImageProperties.Title) ? ImageName : ImageProperties.Title;
            set
            {
                if (ImageProperties.Title != value)
                {
                    ImageProperties.Title = value;
                    _ = ImageProperties.SavePropertiesAsync();
                    OnPropertyChanged();
                }
            }
        }

        public int ImageRating
        {
            get => (int)ImageProperties.Rating;
            set
            {
                if (ImageProperties.Rating != value)
                {
                    ImageProperties.Rating = (uint)value;
                    _ = ImageProperties.SavePropertiesAsync();
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
