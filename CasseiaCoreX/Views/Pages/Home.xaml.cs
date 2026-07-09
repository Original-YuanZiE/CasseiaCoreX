using CasseiaCoreX.ViewModels;
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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        public HomeViewModel ViewModel { get; }

        public Home()
        {
            // 初始化页面与 ViewModel
            ViewModel = new HomeViewModel();
            InitializeComponent();
            DataContext = ViewModel;
            this.Loaded += (s, e) =>
            {
                ViewModel.LoadInfo();
            };


            // 订阅 ViewModel 的事件
            ViewModel.RequestShowGpuList += OnRequestShowGpuList;

            // 启动界面自适应
            RootGrid.SizeChanged += (s, e) => UpdateStateIndicator();
            this.Loaded += (s, e) =>
            {
                if (!MainWindow.mainWindow.isStartupWindowSizeChangeFinish && ActualWidth < 1000)
                {
                    MainWindow.mainWindow.MaxWindow();
                }
                MainWindow.mainWindow.isStartupWindowSizeChangeFinish = true;
            };
        }

        private void UpdateStateIndicator()
        {
            // 自适应 UI 布局
            if (ActualWidth >= 1000 && MainWindow.mainWindow.NavigationView.DisplayMode == NavigationViewDisplayMode.Expanded)
            {
                RootGrid.RowDefinitions.Clear();
                if (RootGrid.ColumnDefinitions.Count < 2)
                {
                    RootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    RootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                RootGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                RootGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                InfoShow.SetValue(Grid.ColumnProperty, 1);
            }
            else
            {
                RootGrid.ColumnDefinitions.Clear();
                if (RootGrid.RowDefinitions.Count < 2)
                {
                    RootGrid.RowDefinitions.Add(new RowDefinition());
                    RootGrid.RowDefinitions.Add(new RowDefinition());
                }
                RootGrid.RowDefinitions[0].Height = new GridLength(500);
                InfoShow.SetValue(Grid.RowProperty, 1);
            }
        }

        private async void OnRequestShowGpuList(object sender, System.EventArgs e)
        {
            // 弹出显卡列表
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "GPU 列表",
                PrimaryButtonText = "好",
                DefaultButton = ContentDialogButton.Primary,
                Content = ViewModel.GPUList
            };
            await dialog.ShowAsync();
        }
    }
}
