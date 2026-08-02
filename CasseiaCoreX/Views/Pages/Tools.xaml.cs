using CasseiaCoreX.Model;
using CasseiaCoreX.ViewModels;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Tools : Page
    {
        public ToolsViewModel ViewModel { get; }

        public Tools()
        {
            // 初始化页面与 ViewModel
            ViewModel = new ToolsViewModel();
            InitializeComponent();
            DataContext = ViewModel;

            // 判断权限
            this.Loaded += CurrentPermission;

            // 注册事件
            ViewModel.BackupDriversEvent += BackupDrivers;
            ViewModel.ImportDriversEvent += ImportDriver;
        }

        private async void CurrentPermission(object sender, RoutedEventArgs e)
        {

            if (!App.IsRunningAsAdmin())
            {
                string title = "权限不足";
                string yes = "允许";
                string no = "拒绝";
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = title;
                dialog.PrimaryButtonText = yes;
                dialog.SecondaryButtonText = no;
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "需要管理员权限才能正常修改此页面的设置项，是否允许应用以管理员身份重新启动？";

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.Verb = "runas";
                    startInfo.FileName = Path.Combine(App.Root, "CasseiaCoreX.exe");

                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {

                    }
                    Application.Current.Exit();
                }
                else
                {
                    MainWindow.mainWindow.FrameNavigation("NavigateHome");
                }

            }
        }

        private async void BackupDrivers(object sender, EventArgs e)
        {
            // 备份驱动
            WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow));
            var picker = new FolderPicker(windowId);

            picker.Title = "选择保存驱动的位置";
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.ViewMode = PickerViewMode.List;

            MainWindow.mainWindow?.ShowAwaitOverlay(true);

            var folder = await picker.PickSingleFolderAsync();

            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            if (folder == null)
            {
                return;
            }
            string path = Path.Combine(folder.Path, $"Drivers_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}");

            var result = await App.ShowDialog(this.XamlRoot,
                "备份驱动",
                $"驱动将被备份至 {path}",
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);
            if (result == ContentDialogResult.Secondary)
            {
                return;
            }

            MainWindow.mainWindow?.ShowAwaitOverlay(true);

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                MainWindow.mainWindow?.ShowAwaitOverlay(false);
                await App.ShowDialog(this.XamlRoot,
                "执行失败，发生错误",
                $"{ex.Message}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);

                return;
            }

            await Task.Run(() => { DismTools.DriverBackup(path); });

            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            await App.ShowDialog(this.XamlRoot,
                "备份完成",
                null,
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }

        private async void ImportDriver(object sender, EventArgs e)
        {
            // 导入驱动
            WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow));
            var picker = new FolderPicker(windowId);

            picker.Title = "选择存放驱动的文件夹";
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.ViewMode = PickerViewMode.List;

            MainWindow.mainWindow?.ShowAwaitOverlay(true);

            var folder = await picker.PickSingleFolderAsync();

            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            if (folder == null)
            {
                return;
            }
            string path = folder.Path;

            var result = await App.ShowDialog(this.XamlRoot,
                "导入驱动",
                $"将导入来自 {path} 及其子目录下的所有驱动，请确保其中不包含来路不明或具有恶意的驱动文件",
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);
            if (result == ContentDialogResult.Secondary)
            {
                return;
            }

            MainWindow.mainWindow?.ShowAwaitOverlay(true);

            await Task.Run(() => { DismTools.DriverImport(path); });

            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            await App.ShowDialog(this.XamlRoot,
                "导入完成",
                null,
                "好",
                null,
                null,
                ContentDialogButton.Primary);

        }
    }
}
