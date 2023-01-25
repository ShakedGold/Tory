using ABI.System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToryNew.UserViewer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ToryNew.Assets.Helper {
    public class BrowseHelper {
        // The event for the button that will pick the files in step 1
        public static async Task<IReadOnlyList<StorageFile>> BrowseFiles<T>() where T : Enum {
            // Create the file picker
            var filePicker = new FileOpenPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = WindowNative.GetWindowHandle(App.Window);

            // Associate the HWND with the file picker
            InitializeWithWindow.Initialize(filePicker, hwnd);

            //file picker settings
            filePicker.SuggestedStartLocation = GetPickerLocationId<T>();

            foreach (var en in Enum.GetValues(typeof(T)).Cast<T>()) {
                filePicker.FileTypeFilter.Add("." + en.ToString());
            }


            var files = await filePicker.PickMultipleFilesAsync();
            if (files.Count == 0) return null;

            return files;
        }

        // The event for the button that will pick the folder in step 1
        public static async Task<IReadOnlyList<StorageFile>> BrowseFolder<T>() where T : Enum {
            var folderPicker = new FolderPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = WindowNative.GetWindowHandle(App.Window);

            // Associate the HWND with the file picker
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            //folder Picker settings
            folderPicker.SuggestedStartLocation = GetPickerLocationId<T>();
            folderPicker.FileTypeFilter.Add("*");

            //pick a folder
            var folder = await folderPicker.PickSingleFolderAsync();

            //if picked a folder continue if not dont go to the next page
            if (folder == null) return null;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

            //pick all the files in a directory
            var files = await folder.GetFilesAsync();
            files = files.ToArray();

            //filter files
            var tempFiles = files.Where(s => {
                var type = s.ContentType.Split("/")[1];
                if(type == "mpeg") type = "mp3";

                return Enum.GetNames(typeof(T))
                            .Contains(type.ToUpper());
            });
            return tempFiles.ToArray();
        }
        private static PickerLocationId GetPickerLocationId<T>() where T : Enum {
            switch(typeof(T).Name.Split("Format")[0]) {
                case "Video":
                    return PickerLocationId.VideosLibrary;
                case "Audio":
                    return PickerLocationId.MusicLibrary;
                case "Document":
                    return PickerLocationId.DocumentsLibrary;
                case "Image":
                    return PickerLocationId.PicturesLibrary;
                default:
                    return PickerLocationId.VideosLibrary;
            }
        }

        public static async Task<string> SaveFolder<T>() where T : Enum {
            var folderPicker = new FolderPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = WindowNative.GetWindowHandle(App.Window);

            // Associate the HWND with the file picker
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            //folder Picker settings
            folderPicker.SuggestedStartLocation = GetPickerLocationId<T>();
            folderPicker.FileTypeFilter.Add("*");

            //pick a folder
            var folder = await folderPicker.PickSingleFolderAsync();

            //if picked a folder continue if not dont go to the next page
            if (folder == null) return "";
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            return folder.Path;
        }
    }
}
