using CasseiaCoreX.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CasseiaCoreX.Views.Pages
{
    public sealed partial class ConvertMediaPage : Page, ViewModels.IInternalWindowPage
    {
        ViewModels.ConvertMediaViewModel ViewModel;

        public ConvertMediaPage()
        {
            ViewModel = new ViewModels.ConvertMediaViewModel();

            InitializeComponent();

            DataContext = ViewModel;

            ViewModel.XamlRoot = this.XamlRoot;

            ViewModel.SelMediaEvt += SelFiles;
            ViewModel.SelFolderEvt += SelFolder;
        }

        public async void SelFiles(object? sender, EventArgs e)
        {
            WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow));
            var picker = new FileOpenPicker(windowId);

            picker.FileTypeChoices.Add("常见媒体文件", ConvertFileModel.MediaConverter.SupportedInputExtensions.Append(".ncm").ToList());
            picker.FileTypeChoices.Add("所有文件", new List<string> { "*" });
            picker.Title = "选择要转换格式的媒体文件（可批量选择）";

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

            folderPicker.Title = "选择转换后媒体文件的保存位置";
            folderPicker.CommitButtonText = "选择文件夹";
            folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

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

            string[] selMedia = ViewModel.SelFilesText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var result = await App.ShowDialog(
                XamlRoot,
                "转换媒体格式",
                $"将转换 {selMedia.Length} 个媒体文件",
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            MainWindow.mainWindow?.ShowWindowOverlay(value: false, onlyCollapsed: true);

            string[] files = selMedia;
            string targetExtension = ViewModel.SelEncodeType.Content?.ToString() ?? ".mp4";
            string outputDirectory = ViewModel.TargetFolder;

            int successCount = 0;
            int failedCount = 0;
            string failedLog = "";

            MainWindow.mainWindow?.ShowAwaitOverlay(true, $"正在转换 {files.Length} 个媒体文件...");

            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string outputFileName = fileName + targetExtension;

                    await ConvertFileModel.MediaConverter.ConvertSingleMedia(file, outputDirectory, outputFileName);

                    successCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    failedLog += $"{DateTime.Now:yyyyMMdd_HHmmss} : {Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(file) + targetExtension)} : {ex.Message}\n";
                }

                MainWindow.mainWindow?.OverlayText = $"正在转换 {files.Length} 个媒体文件...\n进度：{successCount + failedCount}/{files.Length}\n成功: {successCount}，失败: {failedCount}";
            }

            if (failedCount != 0)
            {
                System.IO.File.WriteAllText(Path.Combine(outputDirectory, $"FailedLog_{DateTime.Now:yyyyMMdd_HHmmss}.log"), failedLog);
            }

            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            await App.ShowDialog(
                this.XamlRoot,
                "转换完成",
                $"成功转换 {successCount} 个媒体文件\n失败 {failedCount} 个\n若有失败，可在生成目录查看失败日志文件",
                "好",
                null,
                null,
                ContentDialogButton.Primary);

            MainWindow.mainWindow?.ShowWindowOverlay(false);
        }

        private void ScrollViewer_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "添加到媒体转换列表";
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