using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static CasseiaCoreX.Model.Win32API;

namespace CasseiaCoreX.Model
{
    public static class Win32API
    {
        // 调用 winbrand,dll 以获取 Windows 版本名称
        [DllImport("winbrand.dll", CharSet = CharSet.Unicode)]
        public static extern string BrandingFormatString(string format);

        // 调用 user32.dll 以获取桌面壁纸
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        public const int SPI_GETDESKWALLPAPER = 0x0073;

        // 调用 user32.dll 以更换壁纸
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        // 调用 kernel32.dll 以获取内存大小
        [DllImport("kernel32.dll")]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        // 调用 user32.dll 以获取 GPU 信息
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref PDISPLAY_DEVICEW lpDisplayDevice, uint dwFlags);

        // 调用 user32.dll 以获取显示信息
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettingsEx(string lpszDeviceName, int iModeNum, ref DEVMODEW lpDevMode, uint dwFlags);

        // 内存信息结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

        }

        // GPU 信息结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct PDISPLAY_DEVICEW
        {
            public uint cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        // 获取打印机和显示器信息的结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODEW
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;

            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;

            [StructLayout(LayoutKind.Explicit)]
            public struct DUMMYUNIONNAME
            {
                // 打印机字段
                [FieldOffset(0)]
                public PRINTER_FIELDS DUMMYSTRUCTNAME;

                // 显示器字段
                [FieldOffset(0)]
                public DISPLAY_FIELDS DUMMYSTRUCTNAME2;
            }

            public DUMMYUNIONNAME u1;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;

            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;

            [StructLayout(LayoutKind.Explicit)]
            public struct DUMMYUNIONNAME2
            {
                [FieldOffset(0)]
                public uint dmDisplayFlags;

                [FieldOffset(0)]
                public uint dmNup;
            }

            public DUMMYUNIONNAME2 u2;

            public uint dmDisplayFrequency;


            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;

            public uint dmPanningWidth;
            public uint dmPanningHeight;


            // 打印机专用字段结构
            [StructLayout(LayoutKind.Sequential)]
            public struct PRINTER_FIELDS
            {
                public short dmOrientation;
                public short dmPaperSize;
                public short dmPaperLength;
                public short dmPaperWidth;
                public short dmScale;
                public short dmCopies;
                public short dmDefaultSource;
                public short dmPrintQuality;
            }

            // 显示器专用字段结构
            [StructLayout(LayoutKind.Sequential)]
            public struct DISPLAY_FIELDS
            {
                public POINTL dmPosition;
                public uint dmDisplayOrientation;
                public uint dmDisplayFixedOutput;
            }
        }

        // 用来给多显示器定位的坐标结构，(0, 0) 为主显示器
        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            public int x;
            public int y;
        }
    }



    public class DeviceInfo
    {
        public static string GetCPUName()
        {
            // 获取 CPU 名称
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            if (key != null)
            {
                return key.GetValue("ProcessorNameString") != null ? key.GetValue("ProcessorNameString").ToString() : "获取 CPU 信息失败";
            }
            else return "获取 CPU 信息失败";
        }

        public static string GetWindowsVersion()
        {
            // 获取 Windows 版本号
            if (Environment.OSVersion.VersionString != null)
            {
                return Environment.OSVersion.VersionString;
            }
            else
            {
                return "获取 Windows 版本失败";
            }
        }

        public static string GetWindowsBrandingName()
        {
            // 获取 Windows 版本名称
            try
            {
                string brandName = BrandingFormatString("%WINDOWS_LONG%");
                return brandName ?? "未知 Windows 系统";
            }
            catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
            {
                return "不支持的 Windows 版本";
            }
        }

        public static string GetPCName()
        {
            // 获取电脑名称
            if (Environment.GetEnvironmentVariable("computername") != null)
            {
                return Environment.GetEnvironmentVariable("computername");
            }
            else
            {
                return "获取 Windows 版本失败";
            }
        }

        public static long GetRAM()
        {
            // 获取RAM
            MEMORYSTATUSEX memory = new MEMORYSTATUSEX();
            memory.dwLength = (uint)Marshal.SizeOf(memory);
            if (GlobalMemoryStatusEx(ref memory))
            {
                return (long)memory.ullTotalPhys;
                // 返回值单位为字节，需要除以 1024 的三次方才是 GB
            }
            else return -1;
            // 返回 -1 意味着失败了
        }

        public static string[] GetGPU()
        {
            // 获取GPU信息
            PDISPLAY_DEVICEW pDISPLAY_DEVICEW = new PDISPLAY_DEVICEW();
            pDISPLAY_DEVICEW.cb = (uint)Marshal.SizeOf(pDISPLAY_DEVICEW);
            bool isPDIExist = true;
            List<string> gpu = new List<string>();
            for (uint i = 0; isPDIExist; i++)
            {
                isPDIExist = EnumDisplayDevices(null, i, ref pDISPLAY_DEVICEW, 0);
                gpu.Add(pDISPLAY_DEVICEW.DeviceString);
            }
            gpu = gpu.Distinct().ToList();
            return gpu.ToArray();
            // 返回的 string[] 包含虚拟显示适配器
        }

        public static string GetBIOSInfo(uint infoClass)
        {
            // 获取 BIOS 信息
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
            if (key != null)
            {
                switch (infoClass)
                {
                    case 0:
                        // 获取主板名称
                        return key.GetValue("BaseBoardProduct") != null ? key.GetValue("BaseBoardProduct").ToString() : "主板信息获取失败";

                    case 1:
                        // 获取 BIOS 名称
                        return key.GetValue("BIOSVendor") != null ? key.GetValue("BIOSVendor").ToString() : "BIOS 信息获取失败";

                    default:
                        return $"参数错误，uint infoClass 不能为 {infoClass}";
                }
            }
            else return "无法读取注册表项或注册表项不存在";
        }

        public static int GetDisplayInfo(int infoClass)
        {
            // 获取显示信息
            // 我也不知道这个 API 到底要传几个值，反正能用就好
            DEVMODEW dEVMODEW = new DEVMODEW();
            dEVMODEW.dmSpecVersion = 0x0401;
            dEVMODEW.dmDriverExtra = 0;
            dEVMODEW.dmDriverVersion = 0;
            dEVMODEW.u1.DUMMYSTRUCTNAME2.dmDisplayOrientation = 0;
            dEVMODEW.u1.DUMMYSTRUCTNAME2 = new DEVMODEW.DISPLAY_FIELDS();
            dEVMODEW.u1.DUMMYSTRUCTNAME2.dmPosition = new POINTL { x = 0, y = 0 };
            dEVMODEW.u1.DUMMYSTRUCTNAME2.dmDisplayOrientation = 0;
            dEVMODEW.dmPanningHeight = 0;
            dEVMODEW.dmPanningWidth = 0;
            dEVMODEW.dmReserved1 = 0;
            dEVMODEW.dmReserved2 = 0;
            dEVMODEW.dmSize = (ushort)Marshal.SizeOf(dEVMODEW);

            dEVMODEW.dmFields = 0x00040000 | 0x00080000 | 0x00100000 | 0x00400000;

            switch (infoClass)
            {
                case 0:
                    // 返回水平分辨率
                    if (EnumDisplaySettingsEx(null, -1, ref dEVMODEW, 0x00000002))
                    {
                        return (int)dEVMODEW.dmPelsWidth;
                    }
                    else
                    {
                        return -1;
                    }
                case 1:
                    // 返回垂直分辨率
                    if (EnumDisplaySettingsEx(null, -1, ref dEVMODEW, 0x00000002))
                    {
                        return (int)dEVMODEW.dmPelsHeight;
                    }
                    else
                    {
                        return -1;
                    }
                case 2:
                    // 返回刷新率
                    if (EnumDisplaySettingsEx(null, -1, ref dEVMODEW, 0x00000002))
                    {
                        return (int)dEVMODEW.dmDisplayFrequency;
                    }
                    else
                    {
                        return -1;
                    }
                default:
                    // 参数错误
                    return -2;
            }
        }

        public static bool GetCasseiaOSVer(out string ver, out string deviceCode)
        {
            ver = String.Empty;
            deviceCode = String.Empty;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation");
            if(key != null)
            {
                ver = key.GetValue("CasseiaOSVersion") != null ? key.GetValue("CasseiaOSVersion").ToString() : String.Empty;
                deviceCode = key.GetValue("CasseiaDeviceCode") != null ? key.GetValue("CasseiaDeviceCode").ToString() : String.Empty;
                if(ver != String.Empty)
                {
                    return true;
                }
                else { return false; }
            }
            else
            {
                return false;
            }
        }
    }

    public static class Explorer
    {
        public static bool ShortcutArrowHidden
        {
            // 快捷方式箭头隐藏
            get
            {

                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons");
                if (key != null)
                {
                    object value = key.GetValue("29");
                    key.Close();
                    if (value != null && value.ToString() == "%systemroot%\\System32\\imageres.dll,197")
                    {
                        return true;
                    }
                }
                return false;
            }
            set
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", true);
                if (key == null)
                {
                    RegistryKey tmp = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", true);
                    if (tmp != null)
                    {
                        tmp.CreateSubKey("Shell Icons");
                        tmp = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", true);
                        if (!value)
                        {
                            tmp.DeleteValue("29", false);
                        }
                        else
                        {
                            tmp.SetValue("29", "%systemroot%\\System32\\imageres.dll,197");
                        }
                        tmp.Close();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                if (!value)
                {
                    key.DeleteValue("29", false);
                }
                else
                {
                    key.SetValue("29", "%systemroot%\\System32\\imageres.dll,197");
                }
                key.Close();
                return;
            }
        }

        public static void RestartExplorer()
        {
            // 重启资源管理器
            Process[] p = Process.GetProcessesByName("explorer");
            foreach (Process process in p)
            {
                process.Kill();
            }
            Process.Start(Path.Combine(Environment.GetEnvironmentVariable("windir"), "explorer.exe"));
        }

        public static string Wallpaper
        {
            get
            {
                // 获取壁纸路径
                StringBuilder wallpaperPath = new StringBuilder(260);
                if (SystemParametersInfo(SPI_GETDESKWALLPAPER, wallpaperPath.Capacity, wallpaperPath, 0) > 0)
                {
                    return wallpaperPath.ToString();

                }
                else
                {
                    return "获取壁纸路径失败";
                }
            }
            set
            {
                // 设置壁纸
                if (string.IsNullOrEmpty(value))
                {
                    return; // 文件路径为 Null
                }
                if (File.Exists(value) == false)
                {
                    return; // 文件不存在
                }

                string fileName = Path.GetFullPath(value);
                var nResult = SystemParametersInfo(20, 0, fileName, 1); //更换壁纸
                return;
            }
        }

        public static bool TabletTBUnlocker()
        {
            // 解锁平板模式
            // 虽然我也不知道为什么要这样反复设置一个键值，但有些版本的 Win11 不这样似乎不起作用
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl", true);
            if (key != null)
            {
                key.SetValue("ConvertibleSlateMode", "1");
                key.SetValue("ConvertibleSlateMode", "0");
                key.Close();
            }
            else { return false; }
            key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer", true);
            if (key != null)
            {
                key.SetValue("TabletPostureTaskbar", "1", Microsoft.Win32.RegistryValueKind.DWord);
                key.Close();
            }
            else { return true; }
            key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl", true);
            if (key != null)
            {
                key.SetValue("ConvertibleSlateMode", "1");
                key.Close();
            }
            else { return false; }
            return true;
        }

        public static bool ShowSecOnTaskBar
        {
            // 任务栏显秒
            get
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
                if (key != null)
                {
                    if (key.GetValue("ShowSecondsInSystemClock") != null)
                    {
                        return key.GetValue("ShowSecondsInSystemClock").ToString() == "1" ? true : false;

                    }
                    else
                    {
                        key.Close();
                        return false;
                    }
                }
                else
                {
                    key.Close();
                    return false;
                }
            }
            set
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
                if (key != null)
                {

                    key.SetValue("ShowSecondsInSystemClock", value ? "1" : "0", RegistryValueKind.DWord);
                    key.Close();
                    return;
                }
                else
                {
                    key.Close();
                    return;
                }
            }
        }
    }

    public static class SystemSettings
    {
        public static string CMDAutoRun
        {
            // CMD 自动执行
            get
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Command Processor", true);
                if (key != null)
                {
                    if (key.GetValue("autorun") != null)
                    {
                        return key.GetValue("autorun").ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
                else return null;
            }
            set
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Command Processor", true);
                if (key != null && !string.IsNullOrEmpty(value))
                {
                    key.SetValue("autorun", value, Microsoft.Win32.RegistryValueKind.String);
                    return;
                }
                else if (key != null && string.IsNullOrEmpty(value))
                {
                    if (key.GetValue("autorun") != null)
                    {
                        key.DeleteValue("autorun");
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else return;
            }
        }

        public static bool LongPathsEnabled
        {
            // 启用长路径支持
            get
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Policies", true);
                if (key != null)
                {
                    if (key.GetValue("LongPathsEnabled") != null)
                    {
                        return key.GetValue("LongPathsEnabled").ToString() == "1" ? true : false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else return false;
            }
            set
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Policies", true);
                if (key != null && key.GetValue("LongPathsEnabled") != null)
                {
                    if (value)
                    {
                        key.SetValue("LongPathsEnabled", "1", Microsoft.Win32.RegistryValueKind.DWord);
                        return;
                    }
                    else
                    {
                        key.DeleteValue("LongPathsEnabled");
                        return;
                    }

                }
                else if (key != null && key.GetValue("LongPathsEnabled") == null)
                {
                    if (value)
                    {
                        key.SetValue("LongPathsEnabled", "1", Microsoft.Win32.RegistryValueKind.DWord);
                        return;
                    }
                    else
                    {
                        key.DeleteValue("LongPathsEnabled");
                        return;
                    }
                }
                else return;
            }
        }

        public static bool LogonUIVerboseInfo
        {
            // LogonUI 详细信息
            get
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (key != null)
                {
                    if (key.GetValue("VerboseStatus") != null)
                    {
                        return key.GetValue("VerboseStatus").ToString() == "1" ? true : false;
                    }
                    else
                    {
                        key.Close();
                        return false;
                    }
                }
                else
                {
                    key.Close();
                    return false;
                }
            }
            set
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (key != null)
                {
                    key.SetValue("VerboseStatus", value ? "1" : "0", RegistryValueKind.DWord);
                    key.Close();
                    return;
                }
            }
        }

        public static int UACBehavior
        {
            /* 控制 UAC 行为
             值 (十六进制)	    含义
             0x00000000	        此选项允许“同意管理员”在执行需要权限提升的操作时，无需征得同意或提供凭据。
             0x00000001	        当操作需要权限提升时，此选项会在安全桌面上提示“同意管理员”输入其用户名和密码（或其他有效管理员凭据）。
             0x00000002	        当操作需要权限提升时，此选项会在安全桌面上提示处于“管理员批准模式”的管理员选择“允许”或“拒绝”。如果“同意管理员”选择“允许”，该操作将以最高的可用权限继续执行。“提示征得同意”消除了要求用户输入用户名和密码来执行特权任务的不便。
             0x00000003	        当操作需要权限提升时，此选项会提示“同意管理员”输入其用户名和密码（或其他有效管理员凭据）。 (注：此提示发生在用户当前桌面，非安全桌面)。
             0x00000004	        当操作需要权限提升时，此选项会提示处于“管理员批准模式”的管理员选择“允许”或“拒绝”。如果“同意管理员”选择“允许”，该操作将以最高的可用权限继续执行。“提示征得同意”消除了要求用户输入用户名和密码来执行特权任务的不便。(注：此提示发生在用户当前桌面，非安全桌面)。
             0x00000005	        此选项是默认设置。它用于在操作需要权限提升时（针对任何非 Windows 二进制文件），在安全桌面上提示处于“管理员批准模式”的管理员选择“允许”或“拒绝”。如果“同意管理员”选择“允许”，该操作将以最高的可用权限继续执行。
             */
            get
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (key != null)
                {
                    if (key.GetValue("ConsentPromptBehaviorAdmin") != null)
                    {
                        return Convert.ToInt32(key.GetValue("ConsentPromptBehaviorAdmin").ToString());

                    }
                    else
                    {
                        key.Close();
                        return -1;
                    }
                }
                else
                {
                    key.Close();
                    return -1;
                }
            }
            set
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (key != null)
                {

                    key.SetValue("ConsentPromptBehaviorAdmin", value, RegistryValueKind.DWord);
                    key.Close();
                    return;
                }
                else
                {
                    key.Close();
                    return;
                }
            }
        }

        public static bool SwitchDefender
        {
            // 禁用 Defender
            get
            {
                // 检查主键路径
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", false);
                if (key == null)
                    return false;

                // 检查主键下的值
                if (key.GetValue("DisableAntiSpyware") == null || key.GetValue("DisableAntiSpyware").ToString() != "1" ||
                    key.GetValue("DisableRealtimeMonitoring") == null || key.GetValue("DisableRealtimeMonitoring").ToString() != "1" ||
                    key.GetValue("DisableAntiVirus") == null || key.GetValue("DisableAntiVirus").ToString() != "1" ||
                    key.GetValue("DisableSpecialRunningModes") == null || key.GetValue("DisableSpecialRunningModes").ToString() != "1" ||
                    key.GetValue("DisableRoutinelyTakingAction") == null || key.GetValue("DisableRoutinelyTakingAction").ToString() != "1" ||
                    key.GetValue("ServiceKeepAlive") == null || key.GetValue("ServiceKeepAlive").ToString() != "1")
                {
                    key.Close();
                    return false;
                }
                key.Close();

                // 检查 Real-Time Protection 子项
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", false);
                if (key == null)
                    return false;

                if (key.GetValue("DisableBehaviorMonitoring") == null || key.GetValue("DisableBehaviorMonitoring").ToString() != "1" ||
                    key.GetValue("DisableOnAccessProtection") == null || key.GetValue("DisableOnAccessProtection").ToString() != "1" ||
                    key.GetValue("DisableRealtimeMonitoring") == null || key.GetValue("DisableRealtimeMonitoring").ToString() != "1" ||
                    key.GetValue("DisableScanOnRealtimeEnable") == null || key.GetValue("DisableScanOnRealtimeEnable").ToString() != "1")
                {
                    key.Close();
                    return false;
                }
                key.Close();

                // 检查 Signature Updates 子项
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", false);
                if (key == null)
                    return false;

                if (key.GetValue("ForceUpdateFromMU") == null || key.GetValue("ForceUpdateFromMU").ToString() != "1")
                {
                    key.Close();
                    return false;
                }
                key.Close();

                // 检查 Spynet 子项
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", false);
                if (key == null)
                    return false;

                if (key.GetValue("DisableBlockAtFirstSeen") == null || key.GetValue("DisableBlockAtFirstSeen").ToString() != "1")
                {
                    key.Close();
                    return false;
                }
                key.Close();

                return true;
            }
            set
            {
                if (value)  // 禁用 Windows Defender
                {
                    // 创建或打开主键
                    RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender");
                    key.SetValue("DisableAntiSpyware", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableRealtimeMonitoring", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableAntiVirus", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableSpecialRunningModes", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableRoutinelyTakingAction", 1, RegistryValueKind.DWord);
                    key.SetValue("ServiceKeepAlive", 1, RegistryValueKind.DWord);

                    // 创建或打开 Real-Time Protection 子项并设置值
                    RegistryKey subKey = key.CreateSubKey("Real-Time Protection");
                    subKey.SetValue("DisableBehaviorMonitoring", 1, RegistryValueKind.DWord);
                    subKey.SetValue("DisableOnAccessProtection", 1, RegistryValueKind.DWord);
                    subKey.SetValue("DisableRealtimeMonitoring", 1, RegistryValueKind.DWord);
                    subKey.SetValue("DisableScanOnRealtimeEnable", 1, RegistryValueKind.DWord);
                    subKey.Close();

                    // 创建或打开 Signature Updates 子项并设置值
                    subKey = key.CreateSubKey("Signature Updates");
                    subKey.SetValue("ForceUpdateFromMU", 1, RegistryValueKind.DWord);
                    subKey.Close();

                    // 创建或打开 Spynet 子项并设置值
                    subKey = key.CreateSubKey("Spynet");
                    subKey.SetValue("DisableBlockAtFirstSeen", 1, RegistryValueKind.DWord);
                    subKey.Close();

                    key.Close();
                }
                else  // 启用 Windows Defender
                {
                    // 打开主键
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender", true);
                    if (key != null)
                    {
                        // 删除主键下的值
                        if (key.GetValue("DisableAntiSpyware") != null)
                            key.DeleteValue("DisableAntiSpyware");
                        if (key.GetValue("DisableRealtimeMonitoring") != null)
                            key.DeleteValue("DisableRealtimeMonitoring");
                        if (key.GetValue("DisableAntiVirus") != null)
                            key.DeleteValue("DisableAntiVirus");
                        if (key.GetValue("DisableSpecialRunningModes") != null)
                            key.DeleteValue("DisableSpecialRunningModes");
                        if (key.GetValue("DisableRoutinelyTakingAction") != null)
                            key.DeleteValue("DisableRoutinelyTakingAction");
                        if (key.GetValue("ServiceKeepAlive") != null)
                            key.DeleteValue("ServiceKeepAlive");

                        // 删除 Real-Time Protection 子项及其值
                        RegistryKey subKey = key.OpenSubKey("Real-Time Protection", true);
                        if (subKey != null)
                        {
                            if (subKey.GetValue("DisableBehaviorMonitoring") != null)
                                subKey.DeleteValue("DisableBehaviorMonitoring");
                            if (subKey.GetValue("DisableOnAccessProtection") != null)
                                subKey.DeleteValue("DisableOnAccessProtection");
                            if (subKey.GetValue("DisableRealtimeMonitoring") != null)
                                subKey.DeleteValue("DisableRealtimeMonitoring");
                            if (subKey.GetValue("DisableScanOnRealtimeEnable") != null)
                                subKey.DeleteValue("DisableScanOnRealtimeEnable");
                            subKey.Close();
                            // 删除子项本身（如果它不再包含任何值）
                            key.DeleteSubKey("Real-Time Protection");
                        }

                        // 删除 Signature Updates 子项及其值
                        subKey = key.OpenSubKey("Signature Updates", true);
                        if (subKey != null)
                        {
                            if (subKey.GetValue("ForceUpdateFromMU") != null)
                                subKey.DeleteValue("ForceUpdateFromMU");
                            subKey.Close();
                            key.DeleteSubKey("Signature Updates");
                        }

                        // 删除 Spynet 子项及其值
                        subKey = key.OpenSubKey("Spynet", true);
                        if (subKey != null)
                        {
                            if (subKey.GetValue("DisableBlockAtFirstSeen") != null)
                                subKey.DeleteValue("DisableBlockAtFirstSeen");
                            subKey.Close();
                            key.DeleteSubKey("Spynet");
                        }

                        key.Close();
                    }
                }
            }
        }

        public sealed partial class WindowsUpdate
        {
            // WindowsUpdate 相关组策略
            private RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true);
            private RegistryKey keyAU = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true);

            public WindowsUpdate()
            {

                if (key == null)
                {
                    key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate");
                }

                if (keyAU == null)
                {
                    keyAU = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU");
                }

            }

            public void Close()
            {
                // 操作完后建议执行此方法以避免长时间持有注册表句柄
                key.Close();
                keyAU.Close();
            }

            public int MaxAllowDelayDays
            {
                // 暂停更新的最大天数
                get
                {
                    RegistryKey reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings");
                    if (reg.GetValue("FlightSettingsMaxPauseDays") == null)
                    {
                        reg.Close();
                        return 7;
                    }
                    int result = System.Convert.ToInt32(reg.GetValue("FlightSettingsMaxPauseDays"));
                    reg.Close();
                    return result;
                }
                set
                {
                    RegistryKey reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings");
                    if (reg == null)
                    {
                        reg.Close();
                        return;
                    }
                    reg.SetValue("FlightSettingsMaxPauseDays", value);
                    reg.Close();
                }
            }

            public bool IsEnable
            {
                get
                {
                    if (keyAU != null)
                    {
                        if (keyAU.GetValue("UseWUServer") == null)
                        {
                            return false;
                        }
                        return System.Convert.ToInt32(keyAU.GetValue("UseWUServer")) == 1 ? true : false;

                    }

                    return false;
                }
                set
                {
                    if (keyAU != null)
                    {
                        if (value == false)
                        {
                            keyAU.SetValue("UseWUServer", 0);
                        }
                        else
                        {
                            keyAU.SetValue("UseWUServer", 1);
                        }
                    }
                }
            }

            public string WUServer
            {
                // 更新服务器位置
                get
                {
                    if (key != null)
                    {
                        if (key.GetValue("WUServer") == null)
                        {
                            return null;
                        }
                        return key.GetValue("WUServer").ToString();

                    }

                    return null;
                }
                set
                {
                    if (key != null)
                    {
                        if (value == null)
                        {
                            key.DeleteValue("WUServer", false);
                        }
                        else
                        {
                            key.SetValue("WUServer", value);
                        }
                    }

                }
            }

            public string UpdateServiceUrlAlternate
            {
                // 备用服务器位置
                get
                {
                    if (key != null)
                    {
                        if (key.GetValue("UpdateServiceUrlAlternate") == null)
                        {
                            return null;
                        }
                        return key.GetValue("UpdateServiceUrlAlternate").ToString();

                    }

                    return null;
                }
                set
                {
                    if (key != null)
                    {
                        if (value == null)
                        {
                            key.DeleteValue("UpdateServiceUrlAlternate", false);
                        }
                        else
                        {
                            key.SetValue("UpdateServiceUrlAlternate", value);
                        }
                    }
                }
            }

            public string WUStatusServer
            {
                // 统计服务器位置
                get
                {
                    if (key != null)
                    {
                        if (key.GetValue("WUStatusServer") == null)
                        {
                            return null;
                        }
                        return key.GetValue("WUStatusServer").ToString();

                    }

                    return null;
                }
                set
                {
                    if (key != null)
                    {
                        if (value == null)
                        {
                            key.DeleteValue("WUStatusServer", false);
                        }
                        else
                        {
                            key.SetValue("WUStatusServer", value);
                        }
                    }
                }
            }

        }

        public static class Activate
        {
            static Dictionary<string, string> ProductKeys = new Dictionary<string, string>
            {
                { "Education", "YNMGQ-8RYV3-4PGQ3-C8XTP-7CFBY" },
                { "Education N", "84NGF-MHBT6-FXBX8-QWJK7-DRR8H" },
                { "Enterprise", "XGVPP-NMH47-7TTHJ-W3FW7-8HV2C" },
                { "Enterprise N", "3V6Q6-NQXCX-V8YXR-9QCYV-QPFCT" },
                { "Enterprise LTSB 2015", "FWN7H-PF93Q-4GGP8-M8RF3-MDWWW" },
                { "Enterprise LTSB 2016", "NK96Y-D9CD8-W44CQ-R8YTK-DYJWX" },
                { "Enterprise LTSC 2019", "43TBQ-NH92J-XKTM7-KT3KK-P39PB" },
                { "Enterprise N LTSB 2015", "NTX6B-BRYC2-K6786-F6MVQ-M7V2X" },
                { "Enterprise N LTSB 2016", "2DBW3-N2PJG-MVHW3-G7TDK-9HKR4" },
                { "Home", "YTMG3-N6DKC-DKB77-7M9GH-8HVX7" },
                { "Home N", "4CPRK-NM3K3-X6XXQ-RXX86-WXCHW" },
                { "Home China", "N2434-X9D7W-8PF6X-8DV9T-8TYMD" },
                { "Home Single Language", "BT79Q-G7N6G-PGBYW-4YWX6-6F4BT" },
                { "IoT Enterprise", "XQQYW-NFFMW-XJPBH-K8732-CKFFD" },
                { "IoT Enterprise Subscription", "P8Q7T-WNK7X-PMFXY-VXHBG-RRK69" },
                { "IoT Enterprise LTSC 2021", "QPM6N-7J2WJ-P88HH-P3YRH-YY74H" },
                { "IoT Enterprise LTSC 2024", "CGK42-GYN6Y-VD22B-BX98W-J8JXD" },
                { "IoT Enterprise LTSC Subscription 2024", "N979K-XWD77-YW3GB-HBGH6-D32MH" },
                { "Pro", "VK7JG-NPHTM-C97JM-9MPGT-3V66T" },
                { "Pro N", "2B87N-8KFHP-DKV6R-Y2C8J-PKCKT" },
                { "Pro Education", "8PTT6-RNW4C-6V7J2-C2D3X-MHBPB" },
                { "Pro Education N", "GJTYN-HDMQY-FRR76-HVGC7-QPF8P" },
                { "Pro for Workstations", "DXG7C-N36C4-C4HTG-X4T3X-2YV77" },
                { "Pro N for Workstations", "WYPNQ-8C467-V2W6J-TX4WX-WT2RQ" },
                { "S", "V3WVW-N2PV2-CGWC3-34QGF-VMJ2C" },
                { "S N", "NH9J3-68WK7-6FB93-4K3DF-DJ4F6" },
                { "SE", "KY7PN-VR6RX-83W6Y-6DDYQ-T6R4W" },
                { "SE N", "K9VKN-3BGWV-Y624W-MCRMQ-BHDCD" },
                { "Team", "XKCNC-J26Q9-KFHD2-FKTHY-KD72Y" }
            };

            static Dictionary<string, string> XmlFileName = new Dictionary<string, string>
            {
                { "Education N", "Education.N.xml" },
                { "Education", "Education.xml" },
                { "Enterprise LTSB 2015", "Enterprise.LTSB.2015.xml" },
                { "Enterprise LTSB 2016", "Enterprise.LTSB.2016.xml" },
                { "Enterprise LTSC 2019", "Enterprise.LTSC.2019.xml" },
                { "Enterprise N LTSB 2015", "Enterprise.N.LTSB.2015.xml" },
                { "Enterprise N LTSB 2016", "Enterprise.N.LTSB.2016.xml" },
                { "Enterprise N", "Enterprise.N.xml" },
                { "Enterprise", "Enterprise.xml" },
                { "Home China", "Home.China.xml" },
                { "Home N", "Home.N.xml" },
                { "Home Single Language", "Home.Single.Language.xml" },
                { "Home", "Home.xml" },
                { "IoT Enterprise LTSC 2021", "IoT.Enterprise.LTSC.2021.xml" },
                { "IoT Enterprise LTSC 2024", "IoT.Enterprise.LTSC.2024.xml" },
                { "IoT Enterprise LTSC Subscription 2024", "IoT.Enterprise.LTSC.Subscription.2024.xml" },
                { "IoT Enterprise Subscription", "IoT.Enterprise.Subscription.xml" },
                { "IoT Enterprise", "IoT.Enterprise.xml" },
                { "Pro Education N", "Pro.Education.N.xml" },
                { "Pro Education", "Pro.Education.xml" },
                { "Pro for Workstations", "Pro.for.Workstations.xml" },
                { "Pro N for Workstations", "Pro.N.for.Workstations.xml" },
                { "Pro N", "Pro.N.xml" },
                { "Pro", "Pro.xml" },
                { "S", "Cloud.S.xml" },
                { "S N", "Cloud.S.N.xml" },
                { "SE", "CloudEdition.SE.xml" },
                { "SE N", "CloudEdition.SE.N.xml" },
                { "Team", "Team.xml" }
            };

            public static void ClearKeys()
            {
                // 清除密钥
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();

                p.StandardInput.WriteLine($"slmgr -upk");
                Task.Delay(1000).Wait();
                p.StandardInput.WriteLine("slmgr -cpky");
                Task.Delay(1000).Wait();
                p.StandardInput.WriteLine("slmgr -rearm");
                Task.Delay(1000).Wait();
                p.StandardInput.WriteLine("exit");
                p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }

            public static bool HWID(string selVersion, out string output)
            {
                // 数字激活
                output = String.Empty;
                string GTPath = Path.Combine(Environment.GetEnvironmentVariable("SystemDrive"), "ProgramData", "Microsoft", "Windows", "ClipSVC", "GenuineTicket");
                string SCPath = Path.Combine(App.Root, "Assets", "GenuineTickets", $"{XmlFileName[selVersion]}");

                try
                {
                    if (!Directory.Exists(GTPath))
                    {
                        Directory.CreateDirectory(GTPath);
                    }

                    if (!File.Exists(Path.Combine(GTPath, XmlFileName[selVersion])))
                    {
                        File.Copy(SCPath, Path.Combine(GTPath, XmlFileName[selVersion]));
                    }
                    else
                    {
                        File.Delete(Path.Combine(GTPath, XmlFileName[selVersion]));
                        File.Copy(SCPath, Path.Combine(GTPath, XmlFileName[selVersion]));
                    }
                }
                catch (Exception ex)
                {
                    output = $"无法复制票据文件或文件不存在\n错误信息：{ex.Message}";
                    return false;
                    
                }

                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();

                p.StandardInput.WriteLine($"slmgr -ipk {ProductKeys[selVersion]}");
                Task.Delay(3000).Wait();
                p.StandardInput.WriteLine("slmgr -ato");
                p.StandardInput.WriteLine("exit");
                p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                return true;
            }

            public static void Home2Pro()
            {
                // Home 转 Pro
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();

                p.StandardInput.WriteLine("changepk.exe /ProductKey VK7JG-NPHTM-C97JM-9MPGT-3V66T");
                p.StandardInput.WriteLine("exit");
                p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }

            public static bool BackupKeys(out string ex, out string sysVer, out string keyVer, out string keyClass, out string currentkey)
            {
                // 备份密钥
                ex = String.Empty;
                sysVer = String.Empty;
                keyVer = String.Empty;
                keyClass = String.Empty;
                currentkey = String.Empty;
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (regKey == null)
                {
                    ex = @"找不到注册表项 HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                    return false;
                }

                if (regKey.GetValue("DigitalProductId4") == null)
                {
                    ex = @"找不到注册表键 HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\DigitalProductId4";
                    return false;
                }

                // 开始解密

                byte[] data = (byte[])regKey.GetValue("DigitalProductId4");


                char[] digits = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'M', 'P', 'Q', 'R', 'T', 'V', 'W', 'X', 'Y', '2', '3', '4', '6', '7', '8', '9' };

                const int decodeLength = 29;
                const int decodeStringLength = 15;
                const int numLetters = 24;
                const int keyStartIndex = 808;

                char[] decodedChars = new char[decodeLength];

                int keyEndIndex = keyStartIndex + 15;


                int containsN = (data[keyStartIndex + 14] >> 3) & 1;
                data[keyStartIndex + 14] = (byte)((data[keyStartIndex + 14] & 0xF7) | ((containsN & 2) << 2));


                List<byte> hexPid = new List<byte>();
                for (int i = keyStartIndex; i <= keyEndIndex; i++)
                {
                    hexPid.Add(data[i]);
                }
                for (int i = decodeLength - 1; i >= 0; i--)
                {

                    if ((i + 1) % 6 == 0)
                    {
                        decodedChars[i] = '-';
                    }
                    else
                    {

                        int digitMapIndex = 0;
                        for (int j = decodeStringLength - 1; j >= 0; j--)
                        {
                            int byteValue = (digitMapIndex << 8) | hexPid[j];
                            hexPid[j] = (byte)(byteValue / numLetters);
                            digitMapIndex = byteValue % numLetters;
                            decodedChars[i] = digits[digitMapIndex];
                        }
                    }
                }

                string key = new string(decodedChars);


                if (containsN != 0)
                {
                    int firstLetterIndex = 0;
                    for (int index = 0; index < numLetters; index++)
                    {
                        if (decodedChars[0] != digits[index]) continue;
                        firstLetterIndex = index;
                        break;
                    }
                    string keyWithN = new string(decodedChars);

                    keyWithN = keyWithN.Replace("-", string.Empty).Remove(0, 1);
                    keyWithN = keyWithN.Substring(0, firstLetterIndex) + "N" +
                                    keyWithN.Remove(0, firstLetterIndex);
                    keyWithN = keyWithN.Substring(0, 5) + "-" + keyWithN.Substring(5, 5) + "-" +
                                    keyWithN.Substring(10, 5) + "-" + keyWithN.Substring(15, 5) + "-" +
                                    keyWithN.Substring(20, 5);

                    key = keyWithN;
                }

                byte[] versionArray = new byte[128];
                Buffer.BlockCopy(data, 280, versionArray, 0, 128);
                string version = Encoding.Unicode.GetString(versionArray).Replace("\0", "");

                byte[] typeArray = new byte[128];
                Buffer.BlockCopy(data, 1016, typeArray, 0, 128);
                string type = Encoding.Unicode.GetString(typeArray).Replace("\0", "");

                sysVer = DeviceInfo.GetWindowsBrandingName();
                keyVer = version;
                keyClass = type;
                currentkey = key;

                return true;

                
            }
        }
    }

    public static class DismTools
    {
        public static async void DriverBackup(string path)
        {
            // 驱动备份
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            p.Start();
            p.StandardInput.WriteLine($"DISM /Online /Export-driver /Destination:\"{path}\"&&exit");
            p.WaitForExit();

        }

        public static async void DriverImport(string path)
        {
            // 驱动导入
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            p.Start();
            p.StandardInput.WriteLine($"Pnputil /Add-driver \"{Path.Combine(path, "*.inf")}\" /Subdirs /Install&&exit");
            p.WaitForExit();
        }
    }
}
