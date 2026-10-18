using CasseiaCoreX.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using static System.Net.WebRequestMethods;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConvertImagePage : Page, ViewModels.IInternalWindowPage
    {
        ViewModels.ConvertImageViewModel ViewModel;
        Models.ConvertFileModel.FFMPEGManager ffmpeg;

        public static readonly string[] CommonImageFormats =
        {
            "png",
            "jpg",
            "jpeg",
            "bmp",
            "tiff",
            "webp",
            "gif",
            "apng",
            "avif",
            "ico"
        };
        public ConvertImagePage()
        {
            ViewModel = new ViewModels.ConvertImageViewModel();
            ffmpeg = new();
            InitializeComponent();
            DataContext = ViewModel;

            ViewModel.XamlRoot = this.XamlRoot;

            ViewModel.SelIamgeEvt += SelFiles;
            ViewModel.SelFolderEvt += SelFolder;
        }

        public async void SelFiles(object? sender, EventArgs e)
        {
            WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow));
            var picker = new FileOpenPicker(windowId);

            picker.FileTypeChoices.Add("常见图片文件", CommonImageFormats.Select(ext => $".{ext}").ToList());
            picker.FileTypeChoices.Add("所有文件", new List<string> { "*" });
            picker.Title = "选择要转换格式的图片文件（可批量选择）";
            var files = await picker.PickMultipleFilesAsync();

            if (files == null || files.Count == 0)
            {
                return;
            }

            List<string> selectedFiles = ViewModel.SelFilesText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var file in files)
            {
                if (!selectedFiles.Contains(file.Path, StringComparer.OrdinalIgnoreCase))
                {
                    selectedFiles.Add(file.Path);
                }
            }

            ViewModel.SelFilesText = string.Join(Environment.NewLine, selectedFiles);
        }

        public async void SelFolder(object? sender, EventArgs e)
        {
            WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow));
            var folderPicker = new FolderPicker(windowId);

            folderPicker.Title = "选择转换后图片的保存位置";
            folderPicker.CommitButtonText = "选择文件夹";
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var folder = await folderPicker.PickSingleFolderAsync();

            if (folder == null)
            {
                return;
            }

            ViewModel.TargetFolder = folder.Path;
        }

        public async Task ConfirmCommand()
        {

            if (XamlRoot == null)
            {
                return;
            }

            string[] selImages = ViewModel.SelFilesText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var result = await App.ShowDialog(XamlRoot,
                "转换图片格式",
                $"将转换 {selImages.Count()} 张图片",
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            MainWindow.mainWindow?.ShowWindowOverlay(value: false, onlyCollapsed: true);

            var files = selImages;
            var targetExtension = ViewModel.SelEncodeType.Content;
            var outputDirectory = ViewModel.TargetFolder;

            int successCount = 0;
            int failedCount = 0;
            string failedLog = "";

            MainWindow.mainWindow?.ShowAwaitOverlay(true, $"正在转换 {files.Count()} 张图片...");

            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    string outputFileName = fileName + targetExtension;

                    await ConvertFileModel.ImageConverter.ConvertSingleImage(
                        file,
                        outputDirectory,
                        outputFileName,
                        ViewModel.SelQualityItemIndex);

                    successCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    failedLog += $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")} : {Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(file) + targetExtension)} : {ex.Message}\n";
                }
                MainWindow.mainWindow?.OverlayText = $"正在转换 {files.Count()} 张图片...\n进度：{successCount + failedCount}/{files.Count()}\n成功: {successCount}，失败: {failedCount}";
            }
            if (failedCount != 0)
            {
                System.IO.File.WriteAllText(Path.Combine(outputDirectory, $"FailedLog_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.log"), failedLog);
            }
            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            await App.ShowDialog(
                this.XamlRoot,
                "转换完成",
                $"成功转换 {successCount} 张图片\n失败 {failedCount} 张\n若有失败，可在生成目录查看失败日志文件",
                "好",
                null,
                null,
                ContentDialogButton.Primary);

            MainWindow.mainWindow?.ShowWindowOverlay(false);

        }

        private void ScrollViewer_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "添加到图片转换列表";
        }

        private async void ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            {
                return;
            }

            var items = await e.DataView.GetStorageItemsAsync();
            List<string> selectedFiles = ViewModel.SelFilesText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var item in items)
            {
                if (item is Windows.Storage.StorageFile file && !selectedFiles.Contains(file.Path, StringComparer.OrdinalIgnoreCase))
                {
                    selectedFiles.Add(file.Path);
                }
            }

            ViewModel.SelFilesText = string.Join(Environment.NewLine, selectedFiles);
        }
    }
}
