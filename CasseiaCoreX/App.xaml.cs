using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CasseiaCoreX
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }

        public static string Root
        {
            // App 的根目录
            get
            {
                return AppContext.BaseDirectory.TrimEnd('\\');
            }

        }

        public static string AppVersion
        {
            // App 版本
            get => "1.1.0.0_2608611A_Release";
        }

        public static string AppUpdateChannel
        {
            // 更新通道
            get => "Release";
        }

        public static string AppUpdateVersion
        {
            // 用于 OTA 的版本号
            get => "1.1.0.0";
        }

        public static bool IsRunningAsAdmin()
        {
            // App 权限
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);

        }

        public static async Task<ContentDialogResult> ShowDialog(XamlRoot xamlRoot, string title, Object content, string primary, string secondary, string cancel, ContentDialogButton def)
        {
            // 弹出简单弹窗
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = xamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = primary;
            dialog.SecondaryButtonText = secondary;
            dialog.CloseButtonText = cancel;
            dialog.DefaultButton = def;
            dialog.Content = content;


            return await dialog.ShowAsync();

        }

        public static bool DownloadFile(string uri, string localFileName, out string errorMessage)
        {
            // 下载文件
            errorMessage = null;
            try
            {
                var server = new Uri(uri);
                var p = System.IO.Path.GetDirectoryName(localFileName);
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);

                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(3600);
                var responseMessage = httpClient.GetAsync(server).Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    using (var fs = File.Create(localFileName))
                    {
                        var streamFromService = responseMessage.Content.ReadAsStreamAsync().Result;
                        streamFromService.CopyTo(fs);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return errorMessage == null;
        }
    }
}
