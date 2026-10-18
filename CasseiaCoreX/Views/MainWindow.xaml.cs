using CasseiaCoreX.Pages;
using CasseiaCoreX.Views.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow? mainWindow;
        public static void SetValue(string propertyName)
        {
            mainWindow?.PropertyChanged?.Invoke(mainWindow, new PropertyChangedEventArgs(propertyName));
        }

        private string _OverlayText = "请等待后台操作或弹出窗口返回";
        public string OverlayText
        {
            get => _OverlayText;
            set
            {
                _OverlayText = value;
                SetValue(nameof(OverlayText));
            }
        }

        private SolidColorBrush _backColor = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        public SolidColorBrush BackColor
        {
            get => _backColor;
            set
            {
                _backColor = value;
                SetValue(nameof(BackColor));
            }
        }

        public NavigationView NavigationView => MainNavView;

        public Frame Frame => MainFrame;

        // 窗口句柄
        public string HWND;

        // 是否已完成窗口大小初始化
        public bool isStartupWindowSizeChangeFinish = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool IsRunningAsAdmin()
        {
            // 判断是否以管理员权限运行
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);

        }

        // Win32 API 设置窗口状态
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public MainWindow(string NavigateTarget = "NavigateHome")
        {
            InitializeComponent();

            // 设置窗口样式
            this.ExtendsContentIntoTitleBar = true;
            this.AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
            this.SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };

            // 初始化窗口变量
            mainWindow = this;
            HWND = WinRT.Interop.WindowNative.GetWindowHandle(this).ToString();

            // 在标题上显示权限状态
            if (IsRunningAsAdmin())
            {
                TitleText.Text = $"[Administrator] {TitleText.Text}";
            }

            // 加载主页
            MainNavView.SelectedItem = NavigateHome;
            FrameNavigation(NavigateTarget);
        }

        public void MaxWindow()
        {
            // 最大化窗口
            ShowWindow(System.Convert.ToInt32(HWND), 3);
        }

        private void MainNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // 导航项目点击事件
            var target = args.InvokedItemContainer.Name;
            if (args.IsSettingsInvoked)
            {
                // 保留备用
                // MainFrame.Navigate(typeof(AppSettings));
            }
            else
            {
                FrameNavigation(target);
            }
        }

        public void FrameNavigation(string target)
        {
            // 页面导航
            switch (target)
            {
                case "NavigateHome":
                    MainFrame.Navigate(typeof(Home));
                    MainNavView.SelectedItem = NavigateHome;
                    break;
                case "NavigateSysSettings":
                    MainFrame.Navigate(typeof(SysSettings));
                    MainNavView.SelectedItem = NavigateSysSettings;
                    break;
                case "NavigatePersonalization":
                    MainFrame.Navigate(typeof(Personalization));
                    // MainNavView.SelectedItem = NavigatePersonalization;
                    break;
                case "NavigateTools":
                    MainFrame.Navigate(typeof(Tools));
                    MainNavView.SelectedItem = NavigateTools;
                    break;
                case "NavigateAbout":
                    MainFrame.Navigate(typeof(About));
                    MainNavView.SelectedItem = NavigateAbout;
                    break;
                case "NavigateADB":
                    MainFrame.Navigate(typeof(AndroidDebug));
                    MainNavView.SelectedItem = NavigateADB;
                    break;
                default:
                    MainFrame.Navigate(typeof(Home));
                    MainNavView.SelectedItem = NavigateHome;
                    break;
            }
        }

        public void ShowAwaitOverlay(bool value, string text = "请等待后台操作或弹出窗口返回")
        {
            // 显示或隐藏等待叠加层
            BackColor = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["ThemeColor"]);
            AwaitOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            if (!value) 
            {
                BackColor = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                return;
            }
            OverlayText = text;
        }

        public void ShowWindowOverlay(bool value, Type? page = null, string title = "WindowOverlayTitle", bool onlyCollapsed = false)
        {
            // 显示或隐藏窗口叠加层
            BackColor = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["ThemeColor"]);
            WindowOverlayTitle.Text = title;
            WindowOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            if (!value)
            {
                BackColor = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                if (!onlyCollapsed)
                {
                    WindowOverlayFrame.Content = null;
                }
                return;
            }
            WindowOverlayFrame.Navigate(page);
            WindowOverlay.Focus(FocusState.Programmatic);
        }

        private void OverlayClose_Tapped(object sender, RoutedEventArgs e)
        {
            // 关闭窗口叠加层
            ShowWindowOverlay(false);
        }

        private async void OverlayConfirm_Tapped(object sender, RoutedEventArgs e)
        {
            // 窗口叠加层确定指令
            var pageClass = WindowOverlayFrame.Content;
            ViewModels.IInternalWindowPage internalWindowPage = (ViewModels.IInternalWindowPage)pageClass;
            await internalWindowPage.ConfirmCommand();
        }

        private void WindowOverlay_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                ShowWindowOverlay(false);
            }
        }
    }
}
