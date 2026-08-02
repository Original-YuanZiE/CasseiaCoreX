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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public static MainWindow? mainWindow;

        public NavigationView NavigationView => MainNavView;

        public Frame Frame => MainFrame;

        // 窗口句柄
        public string HWND;

        // 是否已完成窗口大小初始化
        public bool isStartupWindowSizeChangeFinish = false;

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

        public MainWindow()
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
            FrameNavigation("NavigateHome");
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
                    break;
                case "NavigateSysSettings":
                    MainFrame.Navigate(typeof(SysSettings));
                    break;
                case "NavigatePersonalization":
                    MainFrame.Navigate(typeof(Personalization));
                    break;
                case "NavigateTools":
                    MainFrame.Navigate(typeof(Tools));
                    break;
                case "NavigateAbout":
                    MainFrame.Navigate(typeof(About));
                    break;
            }
        }

        public void ShowAwaitOverlay(bool value, string text = "请等待后台操作或弹出窗口返回")
        {
            // 显示或隐藏等待叠加层
            AwaitOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            if (!value) { return; }
            AwaitText.Text = text;
        }
    }
}
