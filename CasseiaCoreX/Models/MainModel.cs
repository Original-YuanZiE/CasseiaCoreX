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
}
