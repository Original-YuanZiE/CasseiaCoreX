using CasseiaCoreX.Model;
using CasseiaCoreX.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.WindowsAppSDK.Runtime;
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
    public sealed partial class SysSettings : Page
    {
        public SysSettingsViewModel ViewModel { get; }

        public SysSettings()
        {
            // 初始化页面与 ViewModel
            ViewModel = new SysSettingsViewModel();
            InitializeComponent();
            DataContext = ViewModel;

            // 判断权限
            this.Loaded += CurrentPermission;

            // 初始化按钮状态
            if (App.IsRunningAsAdmin())
            {
                this.Loaded += (s, e) =>
                {
                    ViewModel.LoadInfo();
                };
            }

            // 订阅 ViewModel 事件
            ViewModel.UnLockTabletModeEvent += UnlockTabletTskbar;
            ViewModel.RestartExplorerEvent += RestartExplorer;
            ViewModel.CMDAutoRunEvent += EditAutoRun;
            ViewModel.UACSettingsEvent += EditUAC;
            ViewModel.MaxDelayUpdateTimeEvent += UpdateMaxDelayDay;
            ViewModel.ClearKeysEvent += ClearKeys;
            ViewModel.HWIDEvent += HWID_Activate;
            ViewModel.Home2ProEvent += HomeToPro;
            ViewModel.BackupKeysEvent += BackupKey;
            ViewModel.SetOEMInfoEvent += EditOEMInfo;
            ViewModel.MASEvent += MASActivate;
        }

        private async void MASActivate(object sender, EventArgs e)
        {
            var result = await App.ShowDialog(this.XamlRoot,
                "Microsoft Activation Scripts (MAS)",
                "使用著名的开源项目 Microsoft Activation Scripts 激活 Windows 与 Office\n您可以选择在线版以联网获取最新版本\n我们也提供本地版来应对网络不佳的情况，但是无法保证功能最新",
                "在线",
                "本地",
                "取消",
                ContentDialogButton.Primary);

            if (result == ContentDialogResult.Primary)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "Powershell.exe";
                psi.Arguments = "irm https://get.activated.win | iex";
                Process.Start(psi);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                Process.Start(Path.Combine(App.Root, "Assets", "MAS_AIO_CN.cmd"));
            }
            else
            {
                return;
            }
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

        private async void UnlockTabletTskbar(object sender, EventArgs e)
        {
            var result = await App.ShowDialog(this.XamlRoot,
                "警告",
                "部分 Windows 11 使用此功能可能导致任务栏永久变成平板任务栏，无法通过设置调整，请三思！\n继续吗？",
                "取消",
                "继续",
                null,
                ContentDialogButton.Primary);
            if (result == ContentDialogResult.Primary)
            {
                return;
            }
            Explorer.TabletTBUnlocker();
            await App.ShowDialog(this.XamlRoot,
                "修改成功",
                null,
                "好",
                null,
                null,
                ContentDialogButton.Primary);
        }

        private async void RestartExplorer(object sender, EventArgs e)
        {

            var result = await App.ShowDialog(this.XamlRoot,
                "需要重启文件资源管理器",
                "要使这项修改生效，你需要重启文件资源管理器，届时桌面、任务栏等系统组件将短暂消失，文件操作进程将被中断",
                "现在重启",
                "不要重启",
                null,
                ContentDialogButton.Primary);

            if (result == ContentDialogResult.Primary)
            {
                Explorer.RestartExplorer();
            }
        }

        private async void EditAutoRun(object sender, EventArgs e)
        {
            string command = ViewModel.CMDAutoRun;
            StackPanel root = new StackPanel();
            root.Spacing = 15;
            root.Orientation = Orientation.Vertical;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "在下面的文本框中输入要自动执行的命令，多个命令请用 && 分隔，此功能无法应用于 Powershell\n例如：cls&&echo Hello, World!";
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.MaxWidth = 500;
            TextBox textBox = new TextBox();
            textBox.Name = "AutoRunCommands";
            textBox.Text = command;
            textBox.TextChanged += (s, args) =>
            {
                command = ((TextBox)s).Text;
            };
            root.Children.Add(textBlock);
            root.Children.Add(textBox);
            var result = await App.ShowDialog(this.XamlRoot,
                "编辑自动执行",
                root,
                "保存",
                null,
                null,
                ContentDialogButton.Primary);
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.CMDAutoRun = command;
            }
        }

        private async void EditUAC(object sender, EventArgs e)
        {
            StackPanel root = new StackPanel();
            root.Spacing = 10;
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = "请选择";
            root.Children.Add(textBlock1);
            ComboBox comboBox1 = new ComboBox();
            string[] strings =
            {
                "当前桌面，直接提权，无需凭据，全局生效",
                "安全桌面，用户授权，需要凭据，全局生效",
                "安全桌面，用户授权，无需凭据，全局生效",
                "当前桌面，用户授权，需要凭据，全局生效",
                "当前桌面，用户授权，无需凭据，全局生效",
                "安全桌面，用户授权，需要凭据，仅第三方"
            };
            for (int i = 0; i < 6; i++)
            {
                comboBox1.Items.Add(strings[i]);
            }
            comboBox1.SelectedIndex = ViewModel.UACSettings;
            comboBox1.SelectionChanged += (s, e) =>
            {
                ViewModel.UACSettings = ((ComboBox)s).SelectedIndex;
            };
            root.Children.Add(comboBox1);

            var result = await App.ShowDialog(this.XamlRoot,
                "编辑 UAC 设置",
                root,
                "保存",
                null,
                null,
                ContentDialogButton.Primary);
        }

        private async void UpdateMaxDelayDay(object sender, EventArgs e)
        {
            StackPanel root = new StackPanel();
            root.Spacing = 15;
            root.Orientation = Orientation.Vertical;
            TextBox textBox = new TextBox();
            textBox.Name = "MaxDays";
            textBox.Text = ViewModel.MaxDelayTime.ToString();
            textBox.TextChanged += (s, args) =>
            {
                try
                {
                    ViewModel.MaxDelayTime = System.Convert.ToInt32(((TextBox)s).Text);
                }
                catch
                {
                    ViewModel.MaxDelayTime = 7;
                }
            };
            root.Children.Add(textBox);

            var result = await App.ShowDialog(this.XamlRoot,
                "最大暂停更新天数",
                root,
                "保存",
                null,
                null,
                ContentDialogButton.Primary);
        }

        private async void ClearKeys(object sender, EventArgs e)
        {
            var result = await App.ShowDialog(this.XamlRoot,
                "警告",
                "此功能可能导致 OEM 密钥丢失，建议提前备份密钥！\n继续吗？",
                "取消",
                "继续",
                null,
                ContentDialogButton.Primary);
            if (result == ContentDialogResult.Primary)
            {
                return;
            }

            SystemSettings.Activate.ClearKeys();
        }

        private async void HWID_Activate(object sender, EventArgs e)
        {
            string selVersion = "Pro";
            StackPanel root = new StackPanel();
            root.Spacing = 10;
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = "请选择目标版本";
            root.Children.Add(textBlock1);
            ComboBox comboBox1 = new ComboBox();
            string[] strings =
            {
                "Education",
                "Education N",
                "Enterprise",
                "Enterprise N",
                "Enterprise LTSB 2015",
                "Enterprise LTSB 2016",
                "Enterprise LTSC 2019",
                "Enterprise N LTSB 2015",
                "Enterprise N LTSB 2016",
                "Home",
                "Home N",
                "Home China",
                "Home Single Language",
                "IoT Enterprise",
                "IoT Enterprise Subscription",
                "IoT Enterprise LTSC 2021",
                "IoT Enterprise LTSC 2024",
                "IoT Enterprise LTSC Subscription 2024",
                "Pro",
                "Pro N",
                "Pro Education",
                "Pro Education N",
                "Pro for Workstations",
                "Pro N for Workstations",
                "S",
                "S N",
                "SE",
                "SE N",
                "Team"
            };
            comboBox1.ItemsSource = strings;
            comboBox1.SelectedIndex = Array.IndexOf(strings, "Pro");
            comboBox1.SelectionChanged += (s, e) =>
            {
                selVersion = strings[((ComboBox)s).SelectedIndex];
            };
            root.Children.Add(comboBox1);

            var result = await App.ShowDialog(this.XamlRoot,
                "数字激活",
                root,
                "激活",
                null,
                "取消",
                ContentDialogButton.Primary);

            if (result == ContentDialogResult.Primary)
            {

                string ex = String.Empty;
                bool success = false;

                MainWindow.mainWindow?.ShowAwaitOverlay(true);

                await Task.Run(() =>
                {
                    success = SystemSettings.Activate.HWID(selVersion, out ex);
                });

                MainWindow.mainWindow?.ShowAwaitOverlay(false);

                if (!success)
                {
                    await App.ShowDialog(this.XamlRoot,
                        "操作失败",
                        $"错误信息：{ex}",
                        "好",
                        null,
                        null,
                        ContentDialogButton.Primary);
                    return;
                }

                await App.ShowDialog(this.XamlRoot,
                    "操作完成",
                    "请在系统设置中检查是否成功激活\n若 slmgr 报错 800A01A8 可以试试在有管理员权限的命令提示符中执行\n\"slmgr -ato\"\n若专业版激活失败，可以试试先执行“家庭版转专业版”功能",
                    "好",
                    null,
                    null,
                    ContentDialogButton.Primary);

            }
        }

        private async void HomeToPro(object sender, EventArgs e)
        {
            var result = await App.ShowDialog(this.XamlRoot,
                "在开始之前",
                "请断开系统的所有网络连接并保存好所有工作再点击继续，电脑可能会重新启动\n若不想继续，请点击取消",
                "取消",
                "继续",
                null,
                ContentDialogButton.Primary);

            if (result == ContentDialogResult.Primary)
            {
                return;
            }

            MainWindow.mainWindow?.ShowAwaitOverlay(true);

            await Task.Run(() => SystemSettings.Activate.Home2Pro());

            MainWindow.mainWindow?.ShowAwaitOverlay(false);

            await App.ShowDialog(this.XamlRoot,
            "操作完成",
            "请在系统设置中检查是否成功转换\n部分电脑可能需要手动重启",
            "好",
            null,
            null,
            ContentDialogButton.Primary);
        }

        private async void BackupKey(object sender, EventArgs e)
        {
            string ex, sysVer, keyVer, keyClass, currentKey;

            if (SystemSettings.Activate.BackupKeys(out ex, out sysVer, out keyVer, out keyClass, out currentKey))
            {
                var result = await App.ShowDialog(this.XamlRoot,
                    "查询结果",
                    $"系统版本：{sysVer}\n密钥版本：{keyVer}\n密钥类型：{keyClass}\n当前密钥：{currentKey}",
                    "保存到桌面",
                    "好",
                    null,
                    ContentDialogButton.Primary);

                if (result == ContentDialogResult.Primary)
                {
                    string path = Path.Combine(Environment.GetEnvironmentVariable("UserProfile"), "Desktop", "Windows 许可证.txt");
                    string content = $"Windows 产品许可证\n\nWindows 版本: {sysVer}\n\n许可证版本: {keyVer}\n\n许可证类型: {keyClass}\n\n许可证: {currentKey}";

                    File.WriteAllText(path, content);
                }
            }
            else
            {
                var result = await App.ShowDialog(this.XamlRoot,
                    "查询失败",
                    $"{ex}",
                    "好",
                    null,
                    null,
                    ContentDialogButton.Primary);
            }
        }

        private async void EditOEMInfo(object sender, EventArgs e)
        {
            Model.SystemSettings.GetOEMInformation(out string manufacturer, out string model, out string supportHours, out string supportPhone, out string supportURL);

            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            stackPanel.Spacing = 10;
            TextBox manufacturerBox = new TextBox();
            manufacturerBox.Header = "制造商";
            manufacturerBox.Text = manufacturer;
            TextBox modelBox = new TextBox();
            modelBox.Header = "型号";
            modelBox.Text = model;
            TextBox supportHoursBox = new TextBox();
            supportHoursBox.Header = "支持时间";
            supportHoursBox.Text = supportHours;
            TextBox supportPhoneBox = new TextBox();
            supportPhoneBox.Header = "支持电话";
            supportPhoneBox.Text = supportPhone;
            TextBox supportURLBox = new TextBox();
            supportURLBox.Header = "支持网站";
            supportURLBox.Text = supportURL;

            stackPanel.Children.Add(manufacturerBox);
            stackPanel.Children.Add(modelBox);
            stackPanel.Children.Add(supportHoursBox);
            stackPanel.Children.Add(supportPhoneBox);
            stackPanel.Children.Add(supportURLBox);

            var result = await App.ShowDialog(this.XamlRoot,
                    "OEM 信息",
                    stackPanel,
                    "保存",
                    "取消",
                    null,
                    ContentDialogButton.Primary);

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            Model.SystemSettings.SetOEMInformation(manufacturerBox.Text, modelBox.Text, supportHoursBox.Text, supportPhoneBox.Text, supportURLBox.Text);
        }
    }
}
