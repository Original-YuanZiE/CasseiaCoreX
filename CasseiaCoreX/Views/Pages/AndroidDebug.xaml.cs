using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CasseiaCoreX.ViewModels;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Windows.Storage.Pickers;
using Microsoft.UI;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AndroidDebug : Page
{

    public AndroidDebugViewModel ViewModel { get; }

    public AndroidDebug()
    {
        // 初始化页面与 ViewModel
        ViewModel = new AndroidDebugViewModel();
        InitializeComponent();
        DataContext = ViewModel;

        ViewModel.SelectedDevice = "当前未选择任何安卓设备";

        this.Loaded += async (s, e) =>
        {
            await InitAdbFiles();
            MainWindow.mainWindow?.ShowAwaitOverlay(true, "请稍等\n\n正在启动 ADB 进程");
            await Task.Run(() =>
            {
                Models.AndroidDebug.InitAdbProcess();
            });
            MainWindow.mainWindow?.ShowAwaitOverlay(false);
            SelectDevice(null, null);
        };

        // 绑定 ViewModel 事件
        ViewModel.KillAdbEvent += KillAdbProcess;
        ViewModel.UpdateAdbEvent += UpdateAdbFiles;
        ViewModel.SelectDeviceEvent += SelectDevice;
        ViewModel.ScreenshotEvent += Screenshot;
        ViewModel.PairDeviceEvent += UsePairCode;
        ViewModel.ConnectDeviceEvent += ConnectDevice;
        ViewModel.InstallAppEvent += InstallApk;
        ViewModel.ManageAppEvent += ManageApp;
    }

    public async void UsePairCode(object sender, EventArgs e)
    {
        // 使用配对码连接安卓设备
        StackPanel sp = new StackPanel();
        sp.Orientation = Orientation.Vertical;
        sp.Spacing = 10;
        TextBox ipTextBox = new TextBox();
        ipTextBox.Header = "设备 IP 地址";
        ipTextBox.PlaceholderText = "例如: 192.168.31.100";
        TextBox hostTextBox = new TextBox();
        hostTextBox.Header = "无线调试端口号";
        hostTextBox.PlaceholderText = "例如: 42997";
        TextBox pairHostTextBox = new TextBox();
        pairHostTextBox.Header = "配对端口号";
        pairHostTextBox.PlaceholderText = "例如: 39007";
        TextBox pairCodeTextBox = new TextBox();
        pairCodeTextBox.Header = "配对码";
        pairCodeTextBox.PlaceholderText = "例如: 114514";
        sp.Children.Add(ipTextBox);
        sp.Children.Add(hostTextBox);
        sp.Children.Add(pairHostTextBox);
        sp.Children.Add(pairCodeTextBox);

        var result = await App.ShowDialog(this.XamlRoot,
            "使用配对码连接安卓设备",
            sp,
            "确定",
            "取消",
            null,
            ContentDialogButton.Primary);

        if (result != ContentDialogResult.Primary) { return; }

        MainWindow.mainWindow?.ShowAwaitOverlay(true, "正在尝试与安卓设备配对");

        string ip = ipTextBox.Text;
        string host = hostTextBox.Text;
        string pairHost = pairHostTextBox.Text;
        string pairCode = pairCodeTextBox.Text;
        bool pairSuccess = false;
        bool connectSuccess = false;
        string connectErrorMessage = string.Empty;
        string errorMessage = string.Empty;
        await Task.Run(() =>
        {
            pairSuccess = Models.AndroidDebug.PairDevices(ip, pairHost, pairCode, out errorMessage);
        });

        MainWindow.mainWindow?.ShowAwaitOverlay(false);

        if (pairSuccess)
        {
            var continueResult = await App.ShowDialog(this.XamlRoot,
                "配对成功",
                "已成功与安卓设备配对，是否继续连接设备以使用无线调试？",
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);

            if (continueResult == ContentDialogResult.Primary)
            {
                MainWindow.mainWindow?.ShowAwaitOverlay(true);
                await Task.Run(() =>
                {
                    connectSuccess = Models.AndroidDebug.ConnectDevices(ip, host, out connectErrorMessage);
                });
                MainWindow.mainWindow?.ShowAwaitOverlay(false);
                if (!connectSuccess)
                {
                    await App.ShowDialog(this.XamlRoot,
                        "连接失败",
                        $"连接失败，错误信息：{connectErrorMessage}",
                        "好",
                        null,
                        null,
                        ContentDialogButton.Primary);
                }
                else
                {
                    await App.ShowDialog(this.XamlRoot,
                        "连接成功",
                        "已成功连接安卓设备，您现在可以使用无线调试功能",
                        "好",
                        null,
                        null,
                        ContentDialogButton.Primary);
                    SelectDevice(null, null);
                }
            }
        }
        else
        {
            await App.ShowDialog(this.XamlRoot,
                "配对失败",
                $"配对失败，错误信息：{errorMessage}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
    }

    public async void ConnectDevice(object sender, EventArgs e)
    {
        // 直接连接无线安卓设备
        StackPanel sp = new StackPanel();
        sp.Orientation = Orientation.Vertical;
        sp.Spacing = 10;
        TextBox ipTextBox = new TextBox();
        ipTextBox.Header = "设备 IP 地址";
        ipTextBox.PlaceholderText = "例如: 192.168.31.100";
        TextBox hostTextBox = new TextBox();
        hostTextBox.Header = "无线调试端口号";
        hostTextBox.PlaceholderText = "例如: 42997";
        sp.Children.Add(ipTextBox);
        sp.Children.Add(hostTextBox);

        var result = await App.ShowDialog(this.XamlRoot,
            "直接连接安卓设备",
            sp,
            "确定",
            "取消",
            null,
            ContentDialogButton.Primary);

        if (result != ContentDialogResult.Primary) { return; }

        MainWindow.mainWindow?.ShowAwaitOverlay(true, "正在尝试连接安卓设备");

        string ip = ipTextBox.Text;
        string host = hostTextBox.Text;
        bool connectSuccess = false;
        string connectErrorMessage = string.Empty;

        await Task.Run(() =>
                {
                    connectSuccess = Models.AndroidDebug.ConnectDevices(ip, host, out connectErrorMessage);
                });

        MainWindow.mainWindow?.ShowAwaitOverlay(false);
        if (!connectSuccess)
        {
            await App.ShowDialog(this.XamlRoot,
                "连接失败",
                $"连接失败，错误信息：{connectErrorMessage}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
        else
        {
            await App.ShowDialog(this.XamlRoot,
                "连接成功",
                "已成功连接安卓设备，您现在可以使用无线调试功能",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
            SelectDevice(null, null);
        }


    }

    public async void UpdateAdbFiles(object sender, EventArgs e)
    {
        // 更新 ADB 文件
        var result = await App.ShowDialog(this.XamlRoot,
            "更新 ADB 文件",
            "确定要下载最新的 ADB 文件吗？",
            "是",
            "否",
            null,
            ContentDialogButton.Primary);
        if (result != ContentDialogResult.Primary) { return; }

        MainWindow.mainWindow?.ShowAwaitOverlay(true, "请稍等\n\n正在下载 ADB 文件");

        bool downloadSuccess = false;
        string errorMessage = string.Empty;

        await Task.Run(() =>
        {
            downloadSuccess = App.DownloadFile("https://googledownloads.cn/android/repository/platform-tools-latest-windows.zip", Path.Combine(App.Root, "Assets", "ADB", "platform-tools-latest-windows.zip"), out errorMessage);
        });

        if (!downloadSuccess)
        {
            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            await App.ShowDialog(this.XamlRoot,
                "更新 ADB 文件失败",
                $"错误信息：{errorMessage}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
            return;
        }

        MainWindow.mainWindow?.ShowAwaitOverlay(true, "请稍等\n\n正在覆盖 ADB 文件");

        try
        {
            ZipFile.ExtractToDirectory(Path.Combine(App.Root, "Assets", "ADB", "platform-tools-latest-windows.zip"), Path.Combine(App.Root, "Assets", "ADB"), true);
            MainWindow.mainWindow?.ShowAwaitOverlay(false);
            await App.ShowDialog(this.XamlRoot,
                "更新 ADB 文件成功",
                "ADB 文件已成功更新",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
        catch (Exception ex)
        {
            MainWindow.mainWindow?.ShowAwaitOverlay(false);
            await App.ShowDialog(this.XamlRoot,
                "解压 ADB 文件失败",
                $"错误信息：{ex.Message}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);

        }
    }

    public async Task InitAdbFiles()
    {
        // 初始化 ADB 文件
        if (File.Exists(Path.Combine(App.Root, "Assets", "ADB", "platform-tools", "adb.exe")))
        {
            //SelectDevice(null, null);
            return;
        }

        if (!File.Exists(Path.Combine(App.Root, "Assets", "ADB", "platform-tools-latest-windows.zip")) && !File.Exists(Path.Combine(App.Root, "Assets", "ADB", "platform-tools", "adb.exe")))
        {
            var downloadResult = await App.ShowDialog(this.XamlRoot,
                "下载 ADB 文件",
                "未检测到 ADB 文件，是否下载最新的 ADB 文件？",
                "是",
                "否",
                null,
                ContentDialogButton.Primary);

            if (downloadResult != ContentDialogResult.Primary)
            {
                MainWindow.mainWindow?.FrameNavigation("NavigateHome");
                return;
            }

            MainWindow.mainWindow?.ShowAwaitOverlay(true, "请稍等\n\n正在下载 ADB 文件");

            bool downloadSuccess = false;
            string errorMessage = string.Empty;

            await Task.Run(() =>
            {
                downloadSuccess = App.DownloadFile("https://googledownloads.cn/android/repository/platform-tools-latest-windows.zip", Path.Combine(App.Root, "Assets", "ADB", "platform-tools-latest-windows.zip"), out errorMessage);
            });

            if (!downloadSuccess)
            {
                MainWindow.mainWindow?.ShowAwaitOverlay(false);

                await App.ShowDialog(this.XamlRoot,
                    "下载 ADB 文件失败",
                    $"错误信息：{errorMessage}",
                    "好",
                    null,
                    null,
                    ContentDialogButton.Primary);
                return;
            }

            MainWindow.mainWindow?.ShowAwaitOverlay(true, "请稍等\n\n正在解压 ADB 文件");

            try
            {
                ZipFile.ExtractToDirectory(Path.Combine(App.Root, "Assets", "ADB", "platform-tools-latest-windows.zip"), Path.Combine(App.Root, "Assets", "ADB"), true);
                MainWindow.mainWindow?.ShowAwaitOverlay(false);
                await App.ShowDialog(this.XamlRoot,
                    "下载 ADB 文件成功",
                    "ADB 文件已成功下载",
                    "好",
                    null,
                    null,
                    ContentDialogButton.Primary);
            }
            catch (Exception ex)
            {
                MainWindow.mainWindow?.ShowAwaitOverlay(false);
                await App.ShowDialog(this.XamlRoot,
                    "解压 ADB 文件失败",
                    $"错误信息：{ex.Message}",
                    "好",
                    null,
                    null,
                    ContentDialogButton.Primary);

            }
        }

        var result = await App.ShowDialog(this.XamlRoot,
            "初始化 ADB 文件",
            "第一次使用该功能需要释放 ADB 文件，是否继续？",
            "是",
            "否",
            null,
            ContentDialogButton.Primary);

        if (result != ContentDialogResult.Primary)
        {
            MainWindow.mainWindow?.FrameNavigation("NavigateHome");
            return;
        }

        try
        {
            ZipFile.ExtractToDirectory(Path.Combine(App.Root, "Assets", "ADB", "platform-tools-latest-windows.zip"), Path.Combine(App.Root, "Assets", "ADB"), true);

            await App.ShowDialog(this.XamlRoot,
                "初始化 ADB 文件成功",
                "ADB 文件已成功释放",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
        catch (Exception ex)
        {
            await App.ShowDialog(this.XamlRoot,
                "初始化 ADB 文件失败",
                $"错误信息：{ex.Message}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);

            MainWindow.mainWindow?.FrameNavigation("NavigateHome");
            return;
        }

    }

    public async void KillAdbProcess(object sender, EventArgs e)
    {
        // 结束 ADB 进程
        var result = await App.ShowDialog(this.XamlRoot,
            "结束 ADB 进程",
            "确定要结束 ADB 进程吗？",
            "是",
            "否",
            null,
            ContentDialogButton.Primary);

        if (result != ContentDialogResult.Primary) { return; }

        if (!Models.AndroidDebug.KillAdbProcess(out string exMessage))
        {
            await App.ShowDialog(this.XamlRoot,
                "结束 ADB 进程失败",
                $"结束 ADB 进程失败，错误信息：{exMessage}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
        else
        {
            await App.ShowDialog(this.XamlRoot,
                "结束 ADB 进程成功",
                "ADB 进程已成功结束",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
            MainWindow.mainWindow?.FrameNavigation("NavigateHome");
        }
    }

    public async void SelectDevice(object sender, EventArgs e)
    {
        // 选择安卓设备
        List<string> devices = new List<string>();
        Models.AndroidDebug.GetDevices(out devices);
        if (devices.Count == 0)
        {
            await App.ShowDialog(this.XamlRoot,
                "未检测到可用安卓设备",
                "请确保已连接安卓设备、开启 USB 调试模式并授权此电脑",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
            return;
        }

        ComboBox cb = new ComboBox();
        cb.ItemsSource = devices;
        cb.SelectedItem = devices[0];
        var result = await App.ShowDialog(this.XamlRoot,
            "选择安卓设备",
            cb,
            "确定",
            null,
            null,
            ContentDialogButton.Primary);
        if (result != ContentDialogResult.Primary) { return; }
        ViewModel.SelectedDevice = cb.SelectedItem.ToString();
    }

    public async void Screenshot(object sender, EventArgs e)
    {
        // 截图
        MainWindow.mainWindow?.ShowAwaitOverlay(true, "请等待 ADB 返回");
        string errorMessage = string.Empty;
        string filePath = string.Empty;
        string fileName = string.Empty;
        await Task.Run(() =>
        {
            Models.AndroidDebug.ScreenShot(ViewModel.SelectedDevice, out errorMessage, out filePath, out fileName);
        });
        MainWindow.mainWindow?.ShowAwaitOverlay(false);

        if (!File.Exists(filePath))
        {
            await App.ShowDialog(this.XamlRoot,
                "截图失败",
                $"报错信息：{errorMessage}",
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
        else
        {
            var result = await App.ShowDialog(this.XamlRoot,
                "截图成功",
                $"截图已保存至：{filePath}",
                "好",
                "另存为",
                null,
                ContentDialogButton.Primary);
            if (result == ContentDialogResult.Secondary)
            {
                WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow));
                var savePicker = new FileSavePicker(windowId);
                //savePicker.SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                //savePicker.FileTypeChoices.Add("PNG 图片", new List<string>() { ".png" });
                //savePicker.FileTypeChoices.Add("所有文件", new List<string>() { ".*" });
                savePicker.DefaultFileExtension = ".png";
                savePicker.SuggestedFileName = fileName;

                MainWindow.mainWindow?.ShowAwaitOverlay(true);
                var pathResult = await savePicker.PickSaveFileAsync();
                if (pathResult != null)
                {
                    string savePath = pathResult.Path;
                    File.Copy(filePath, savePath, true);
                }

                MainWindow.mainWindow?.ShowAwaitOverlay(false);
            }
        }
    }

    public async void InstallApk(object sender, EventArgs e)
    {
        StackPanel sp = new StackPanel();
        sp.Orientation = Orientation.Vertical;
        sp.Spacing = 10;
        Grid filePathGrid = new Grid();
        TextBox filePathText = new TextBox();
        filePathText.Header = "APK 文件路径";
        filePathText.Padding = new Thickness(5,5,20,0);
        filePathText.TextAlignment = TextAlignment.Left;
        filePathText.VerticalContentAlignment = VerticalAlignment.Center;
        Button browseButton = new Button();
        browseButton.Content = new FontIcon() { Glyph = "\uE8E5" };
        browseButton.Background = new SolidColorBrush(Colors.Transparent);
        browseButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
        browseButton.VerticalAlignment = VerticalAlignment.Bottom;
        browseButton.HorizontalAlignment = HorizontalAlignment.Right;
        browseButton.Click += async (s, e) =>
        {
            WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow));
            var picker = new FileOpenPicker(windowId);
            picker.FileTypeFilter.Add(".apk");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                filePathText.Text = file.Path;
            }
        };
        TextBox uidText = new TextBox();
        uidText.Header = "指定 UserId（可留空）";
        uidText.Padding = new Thickness(5,5,20,0);
        uidText.VerticalContentAlignment = VerticalAlignment.Center;
        filePathGrid.Children.Add(filePathText);
        filePathGrid.Children.Add(browseButton);
        sp.Children.Add(filePathGrid);
        sp.Children.Add(uidText);

        var result = await App.ShowDialog(this.XamlRoot,
            "安装 APK 文件",
            sp,
            "确定",
            "取消",
            null,
            ContentDialogButton.Primary);

        string filePath = filePathText.Text;
        string uId = uidText.Text;
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        MainWindow.mainWindow?.ShowAwaitOverlay(true, "请稍等\n正在尝试安装 App");

        bool success = false;
        string output = String.Empty;

        await Task.Run(() =>
        {
            success =  Models.AndroidDebug.InstallApp(filePath, ViewModel.SelectedDevice, uId, out output);
        });

        MainWindow.mainWindow?.ShowAwaitOverlay(false);

        if (success)
        {
            await App.ShowDialog(this.XamlRoot,
                "命令执行完成",
                null,
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
        else
        {
            await App.ShowDialog(this.XamlRoot,
                "命令执行失败",
                output,
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
    }

    public void ManageApp(object sender, EventArgs e)
    {
        Models.AndroidDebug.GetDevices(out List<string> devList);
        AndroidAppManagement aam = new AndroidAppManagement(devList, ViewModel.SelectedDevice);
        aam.Activate();
    }
}
