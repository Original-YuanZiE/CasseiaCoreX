using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        private Window? _window;

        public string ConsoleGreeting =
        "   ____                              _         \r\n  / ___|   __ _   ___   ___    ___  (_)   __ _ \r\n | |      / _` | / __| / __|  / _ \\ | |  / _` |\r\n | |___  | (_| | \\__ \\ \\__ \\ |  __/ | | | (_| |\r\n  \\____|  \\__,_| |___/ |___/  \\___| |_|  \\__,_|\r\n                                               ";

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
            string[] cmdArgs = Environment.GetCommandLineArgs();
            if (EnableDevConsole)
            {
                AllocConsole();
                Console.Title = "DevConsole By Casseia";
                Console.InputEncoding = System.Text.Encoding.UTF8;
                Console.OutputEncoding = System.Text.Encoding.UTF8;
            }
            
            Console.WriteLine($"Connected to Console\nAdministrator: {IsRunningAsAdmin()}\nArgs: {string.Join(" ", cmdArgs)}\nTime: {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss dddd")}\nApp Version: {AppVersion}\n\n{ConsoleGreeting}");

            int r = ResolveArgs(cmdArgs);
            switch (r)
            {
                case 0:
                    _window = new MainWindow();
                    _window.Activate();
                    break;
                case 1:
                    _window = new MainWindow("NavigateSysSettings");
                    _window.Activate();
                    break;
                case 2:
                    _window = new MainWindow("NavigateTools");
                    _window.Activate();
                    break;
                case 3:
                    _window = new MainWindow("NavigateAbout");
                    _window.Activate();
                    break;
                case 4:
                    _window = new MainWindow("NavigateADB");
                    _window.Activate();
                    break;
            }
        }

        private static int ResolveArgs(string[] args)
        {
            // 解析启动参数
            if (args == null || args.Length == 0)
            {
                return 0;
            }

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-Home":
                            return 0;
                        case "-SysSettings":
                            return 1;
                        case "-Tools":
                            return 2;
                        case "-About":
                            return 3;
                        case "-AndroidDebug":
                            return 4;
                    }
                }
            }
            return 0;
        }

        public static string Root
        {
            // App 的根目录
            get
            {
                return AppContext.BaseDirectory.TrimEnd('\\');
            }

        }

        public static bool EnableDevConsole
        {
            get
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Casseia\CasseiaCoreX");
                return key?.GetValue("DevConsole") is int value && value == 1;
            }
            set
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Casseia\CasseiaCoreX");
                key.SetValue("DevConsole", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static string AppVersion
        {
            // App 版本
            get => "1.5.0.1_2610322S_Release";
        }

        public static string AppUpdateChannel
        {
            // 更新通道
            get => "Release";
        }

        public static string AppUpdateVersion
        {
            // 用于 OTA 的版本号
            get => "1.5.0.1";
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
