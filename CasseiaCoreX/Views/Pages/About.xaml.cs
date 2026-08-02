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
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class About : Page
{
    public About()
    {
        InitializeComponent();

        // 启动界面自适应
        RootGrid.SizeChanged += (s, e) => UpdateStateIndicator();

        this.Loaded += (s, e) =>
        {
            LoadInfo();
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

    private void LoadInfo()
    {
        LogoIMG.Source = new BitmapImage(new Uri(Path.Combine(App.Root, "CasseiaCoreXIcon.png")));
        CasseiaVerShow.Text = App.AppVersion;
        UpdateChannelShow.Text = App.AppUpdateChannel;
    }

    private void AuthorHome_Click(object sender, RoutedEventArgs e)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "https://github.com/Original-YuanZiE",
            UseShellExecute = true,
        };
        Process.Start(psi);
    }

    private void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {

    }
}
