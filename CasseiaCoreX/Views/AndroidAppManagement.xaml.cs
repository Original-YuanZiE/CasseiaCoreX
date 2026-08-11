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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static CasseiaCoreX.Models.AndroidDebug;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public class AppInfoClass
{
    public string AppName { get; set; }
    public string ApkPath { get; set; }
    public string Package { get; set; }
}
public sealed partial class AndroidAppManagement : Window, INotifyPropertyChanged
{

    public static AndroidAppManagement? androidAppManagement;

    public List<string> devicesList;
    public string selectedDeviceId;

    public List<AppInfoClass> AppInfoList;
    public List<AppInfoClass> FilteredAppInfoList;
    IEnumerable<AppInfoClass> filtered;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<AppInfoClass> AppList { get; set; }

    public AndroidAppManagement(List<string> devices, string SelectedDeviceId)
    {
        InitializeComponent();

        // 设置窗口样式
        this.ExtendsContentIntoTitleBar = true;
        this.AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
        TitleText.Text += $" - {SelectedDeviceId}";

        androidAppManagement = this;
        devicesList = devices;
        selectedDeviceId = SelectedDeviceId;

        AppInfoClass appInfoClass = new AppInfoClass();
        List<string> tmp = new List<string>();
        AppInfoList = new List<AppInfoClass>();
        AppList = new ObservableCollection<AppInfoClass>(AppInfoList);

        this.MainGrid.Loaded += async (s, e) =>
        {
            string test = string.Empty;
            tmp = GetPackageList().Result;
            Dictionary<string, string> tmpd = new Dictionary<string, string>();
            ShowAwaitOverlay(true, "请稍等，正在获取 App 名称\n\n对于首次连接的设备，这可能需要几分钟");
            await Task.Run(() =>
            {
                if (File.Exists(Path.Combine(App.Root, "Assets", "ADB", "DevicesAppListCache", $"{selectedDeviceId}.txt")))
                {
                    string cache = File.ReadAllText(Path.Combine(App.Root, "Assets", "ADB", "DevicesAppListCache", $"{selectedDeviceId}.txt"));
                    string[] lines = cache.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (line.Contains("<Split>") && tmp.Contains(line.Trim().Split("<Split>")[0]))
                        {
                            tmpd.Add(line.Trim().Split("<Split>")[0], line.Trim().Split("<Split>")[1]);
                        }
                    }
                    foreach (string pack in tmp)
                    {
                        if (!tmpd.ContainsKey(pack))
                        {
                            var tmpd1 = PairPackageWithName(PairPackageWithApk().Result, pack).Result;
                            tmpd.Add(pack, tmpd1[pack]);
                        }
                    }
                }
                else
                {
                    tmpd = PairPackageWithName(PairPackageWithApk().Result).Result;
                }
            });
            var sb = new StringBuilder();
            foreach (string pack in tmp)
            {

                string appName = tmpd.TryGetValue(pack, out string name) ? name : "未知应用";

                AppInfoList.Add(new AppInfoClass { Package = pack, AppName = appName });
            }
            AppListView.ItemsSource = AppInfoList;
            SaveAppList(tmpd);
            ShowAwaitOverlay(false);
        };
    }

    public void ShowAwaitOverlay(bool value, string text = "请等待后台操作或弹出窗口返回")
    {
        // 显示或隐藏等待叠加层
        AwaitOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        if (!value) { return; }
        AwaitText.Text = text;
    }





    public async Task<List<string>> GetPackageList()
    {
        ProcessStartInfo pi = new ProcessStartInfo();
        pi.FileName = AdbPath;
        pi.Arguments = $"-s {selectedDeviceId} shell pm list package";
        pi.UseShellExecute = false;
        pi.WindowStyle = ProcessWindowStyle.Hidden;
        pi.RedirectStandardOutput = true;
        var p = Process.Start(pi);
        string stdout = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        List<string> result = new List<string>();
        string[] lines = stdout.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (!line.ToString().StartsWith("package:"))
            {
                continue;
            }
            else
            {
                result.Add(line.Substring(8));
            }
        }
        return result;
    }

    public async Task<Dictionary<string, string>> PairPackageWithApk()
    {
        // 匹配包名与安装包
        ProcessStartInfo pi = new ProcessStartInfo();
        pi.FileName = AdbPath;
        pi.Arguments = $"-s {selectedDeviceId} shell pm list package -f";
        pi.UseShellExecute = false;
        pi.WindowStyle = ProcessWindowStyle.Hidden;
        pi.RedirectStandardOutput = true;
        var p = Process.Start(pi);
        string stdout = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        var dict = new Dictionary<string, string>();
        var lines = stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (!line.StartsWith("package:")) { continue; }
            string content = line.Substring(8);
            int equalIndex = content.LastIndexOf('=');
            if (equalIndex == -1) { continue; }
            string apkPath = content.Substring(0, equalIndex);
            string packageName = content.Substring(equalIndex + 1);
            dict[packageName] = apkPath;
        }
        return dict;
    }

    public async Task<Dictionary<string, string>> PairPackageWithName(Dictionary<string, string> apkDict, string SetAppPackage = null)
    {
        // 匹配包名与 App 名称
        string aaptPath = Path.Combine(App.Root, "Assets", "ADB", "aapt-arm-pie");

        async Task<string> RunAdbCommand(string arg)
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = AdbPath;
            pi.Arguments = $"-s {selectedDeviceId} {arg}";
            pi.UseShellExecute = false;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.RedirectStandardOutput = true;
            var p = Process.Start(pi);
            string stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return stdout;
        }

        await RunAdbCommand($"push {aaptPath} /data/tmp/aapt-arm-pie");
        await RunAdbCommand($"shell chmod 0755 /data/tmp/aapt-arm-pie");

        var dict = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(SetAppPackage))
        {
            string stdout = string.Empty;
            await Task.Run(() =>
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.FileName = "cmd.exe";
                pi.Arguments = $"/c {AdbPath} -s {selectedDeviceId} shell /data/tmp/aapt-arm-pie d badging \"{apkDict[SetAppPackage]}\" | findstr \"application-label: application-label-zh-CN\"";
                pi.UseShellExecute = false;
                pi.WindowStyle = ProcessWindowStyle.Hidden;
                pi.StandardOutputEncoding = Encoding.UTF8;
                pi.RedirectStandardOutput = true;
                var p = Process.Start(pi);
                stdout = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            });
            string tmp;
            try
            {
                if (!stdout.Contains("application-label-zh-CN"))
                {
                    tmp = stdout.Split("-label:'")[1].Trim('\r').Trim('\n').Trim().TrimEnd('\'');
                }
                else
                {
                    tmp = stdout.Split("zh-CN:'")[1].Trim('\r').Trim('\n').Trim().TrimEnd('\'');
                }
            }
            catch
            {
                tmp = SetAppPackage;
            }

            dict.Add(SetAppPackage, tmp);

        }
        else
        {
            foreach (var key in apkDict)
            {
                string stdout = string.Empty;
                await Task.Run(() =>
                {
                    ProcessStartInfo pi = new ProcessStartInfo();
                    pi.FileName = "cmd.exe";
                    pi.Arguments = $"/c {AdbPath} -s {selectedDeviceId} shell /data/tmp/aapt-arm-pie d badging \"{key.Value}\" | findstr \"application-label: application-label-zh-CN\"";
                    pi.UseShellExecute = false;
                    pi.WindowStyle = ProcessWindowStyle.Hidden;
                    pi.StandardOutputEncoding = Encoding.UTF8;
                    pi.RedirectStandardOutput = true;
                    var p = Process.Start(pi);
                    stdout = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                });
                string tmp;
                try
                {
                    if (!stdout.Contains("application-label-zh-CN"))
                    {
                        tmp = stdout.Split("-label:'")[1].Trim('\r').Trim('\n').Trim().TrimEnd('\'');
                    }
                    else
                    {
                        tmp = stdout.Split("zh-CN:'")[1].Trim('\r').Trim('\n').Trim().TrimEnd('\'');
                    }
                }
                catch
                {
                    tmp = key.Key;
                }

                dict.Add(key.Key, tmp);
            }
        }


        await RunAdbCommand($"rm /data/tmp/aapt-arm-pie");
        return dict;
    }

    public async Task SaveAppList(Dictionary<string, string> dict)
    {
        // 列表缓存
        string text = string.Empty;
        string savePath = Path.Combine(App.Root, "Assets", "ADB", "DevicesAppListCache");
        Directory.CreateDirectory(savePath);
        foreach (var key in dict)
        {
            text += $"{key.Key}<Split>{key.Value}\n";
        }
        File.WriteAllText(Path.Combine(savePath, $"{selectedDeviceId}.txt"), text);
    }

    private void SearchAppBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // 搜索
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) { return; }
        string keyword = sender.Text?.Trim();

        if (string.IsNullOrEmpty(keyword))
        {
            filtered = AppInfoList;
        }
        else
        {
            filtered = AppInfoList.Where(app =>
                app.Package.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (app.AppName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }
        AppListView.ItemsSource = filtered.ToList();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        // 刷新缓存
        var result = await App.ShowDialog(this.MainGrid.XamlRoot,
                "刷新列表缓存",
                "这可能需要几分钟，就像第一次读取这台设备的应用列表那样",
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);
        if (result != ContentDialogResult.Primary) { return; }
        ShowAwaitOverlay(true, "请稍等\n\n正在重新建立应用列表，这可能需要几分钟");
        await Task.Run(() =>
        {
            var tmpd = PairPackageWithName(PairPackageWithApk().Result).Result;
            SaveAppList(tmpd);
        });
        ShowAwaitOverlay(false);
        AndroidAppManagement androidAppManagement = new AndroidAppManagement(devicesList, selectedDeviceId);
        androidAppManagement.Activate();
        this.Close();
    }

    private async void OpenApp_Click(object sender, RoutedEventArgs e)
    {
        // 打开应用
        ProcessStartInfo pi = new ProcessStartInfo();
        pi.FileName = AdbPath;
        pi.Arguments = $"-s {selectedDeviceId} shell monkey -p {AppPackDisp.Text} -c android.intent.category.LAUNCHER 1";
        pi.UseShellExecute = false;
        pi.WindowStyle = ProcessWindowStyle.Hidden;
        pi.StandardOutputEncoding = Encoding.UTF8;
        pi.RedirectStandardOutput = true;
        var p = Process.Start(pi);
        p.StandardOutput.ReadToEnd();
        p.WaitForExit();
    }

    private async void UninstallApp_Click(object sender, RoutedEventArgs e)
    {
        // 卸载应用

        StackPanel sp = new StackPanel();
        sp.Orientation = Orientation.Vertical;
        sp.Spacing = 10;
        TextBlock tb = new TextBlock();
        tb.Text = "卸载后其所有数据也将被删除";
        TextBox uidBox = new TextBox();
        uidBox.Header = "指定用户 ID（非系统应用可留空）";
        sp.Children.Add(tb);
        sp.Children.Add(uidBox);
        var result = await App.ShowDialog(this.MainGrid.XamlRoot,
                $"卸载 {AppNameDisp.Text}",
                sp,
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);

        if (result != ContentDialogResult.Primary) { return; }

        string uidArg = string.Empty;
        uidArg = string.IsNullOrEmpty(uidBox.Text) ? "" : $" --user {uidBox.Text}";

        ProcessStartInfo pi = new ProcessStartInfo();
        pi.FileName = AdbPath;
        pi.Arguments = $"-s {selectedDeviceId} shell pm uninstall{uidArg} {AppPackDisp.Text}";
        pi.UseShellExecute = false;
        pi.WindowStyle = ProcessWindowStyle.Hidden;
        pi.StandardOutputEncoding = Encoding.UTF8;
        pi.RedirectStandardOutput = true;
        var p = Process.Start(pi);
        p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        if (AppListView.SelectedItem is AppInfoClass selectedApp && AppPackDisp.Text == selectedApp.Package)
        {
            AppInfoList.Remove(selectedApp);
            AppListView.SelectedItem = null;
            var newList = new List<AppInfoClass>();
            if (!string.IsNullOrEmpty(SearchAppBox.Text))
            {

                newList = filtered.ToList();
                newList.Remove(selectedApp);
                AppListView.ItemsSource = newList;
            }
            else
            {
                AppListView.ItemsSource = AppInfoList.ToList();
            }
        }
    }

    private async void ForceStopApp_Click(object sender, RoutedEventArgs e)
    {
        // 强行停止
        var result = await App.ShowDialog(this.MainGrid.XamlRoot,
                $"强行停止 {AppNameDisp.Text}",
                "强行停止某个应用可能会导致其出现异常",
                "继续",
                "取消",
                null,
                ContentDialogButton.Primary);

        if (result != ContentDialogResult.Primary) { return; }

        ProcessStartInfo pi = new ProcessStartInfo();
        pi.FileName = AdbPath;
        pi.Arguments = $"-s {selectedDeviceId} shell am force-stop {AppPackDisp.Text}";
        pi.UseShellExecute = false;
        pi.WindowStyle = ProcessWindowStyle.Hidden;
        pi.StandardOutputEncoding = Encoding.UTF8;
        pi.RedirectStandardOutput = true;
        var p = Process.Start(pi);
        p.StandardOutput.ReadToEnd();
        p.WaitForExit();
    }

    private void AppListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 选择的应用改变
        if (AppListView.SelectedItem is AppInfoClass selectedApp)
        {

            AppNameDisp.Text = selectedApp.AppName ?? "未知";
            AppPackDisp.Text = selectedApp.Package;
            AppInfoGrid.Visibility = Visibility.Visible;
        }
        else
        {

            AppNameDisp.Text = "未选择应用";
            AppPackDisp.Text = "包名";
            AppInfoGrid.Visibility = Visibility.Collapsed;
        }

    }

    private async void PullApk_Click(object sender, RoutedEventArgs e)
    {
        // 提取安装包
        string ApkOriPath = Path.Combine(App.Root, "Assets", "ADB", $"{AppNameDisp}.apk");
        ProcessStartInfo pi = new ProcessStartInfo();
        pi.FileName = AdbPath;
        pi.Arguments = $"-s {selectedDeviceId} pull {PairPackageWithApk().Result[AppPackDisp.Text]} {ApkOriPath}";
        pi.UseShellExecute = false;
        pi.WindowStyle = ProcessWindowStyle.Hidden;
        pi.StandardOutputEncoding = Encoding.UTF8;
        pi.RedirectStandardOutput = true;
        var p = Process.Start(pi);
        p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        if (File.Exists(ApkOriPath))
        {
            WindowId windowId = new WindowId((ulong)WinRT.Interop.WindowNative.GetWindowHandle(AndroidAppManagement.androidAppManagement));
            var savePicker = new FileSavePicker(windowId);
            savePicker.DefaultFileExtension = ".apk";
            savePicker.SuggestedFileName = $"{AppNameDisp.Text}.apk";

            ShowAwaitOverlay(true);
            var pathResult = await savePicker.PickSaveFileAsync();
            if (pathResult != null)
            {
                string savePath = pathResult.Path;
                File.Move(ApkOriPath, savePath, true);
            }
            else
            {
                File.Delete(ApkOriPath);
            }

            ShowAwaitOverlay(false);
        }
        else
        {
            await App.ShowDialog(this.MainGrid.XamlRoot,
                $"提取失败",
                null,
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }
    }
}
