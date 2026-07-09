using CasseiaCoreX.Model;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static CasseiaCoreX.Model.DeviceInfo;
using static CasseiaCoreX.Model.Explorer;

namespace CasseiaCoreX.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {

        private BitmapImage _wallpaper;
        public BitmapImage Wallpaper
        {
            get => _wallpaper;
            set => Set(ref _wallpaper, value);
        }


        private string _cpuName = "null";
        public string CPUName
        {
            get => _cpuName;
            set => Set(ref _cpuName, value);
        }


        private string _ramInfo = "null";
        public string RAMInfo
        {
            get => _ramInfo;
            set => Set(ref _ramInfo, value);
        }


        private string _gpuName = "null";
        public string GPUName
        {
            get => _gpuName;
            set => Set(ref _gpuName, value);
        }


        private string _gpuList = string.Empty;
        public string GPUList
        {
            get => _gpuList;
            set => Set(ref _gpuList, value);
        }


        private string _windowsBrandName = "Windows 版本";
        public string WindowsBrandName
        {
            get => _windowsBrandName;
            set => Set(ref _windowsBrandName, value);
        }


        private string _windowsVersion = "null";
        public string WindowsVersion
        {
            get => _windowsVersion;
            set => Set(ref _windowsVersion, value);
        }


        private string _libVersion = "null";
        public string LibVersion
        {
            get => _libVersion;
            set => Set(ref _libVersion, value);
        }


        private string _baseBoardInfo = "null";
        public string BaseBoardInfo
        {
            get => _baseBoardInfo;
            set => Set(ref _baseBoardInfo, value);
        }


        private string _displayInfoText = "null";
        public string DisplayInfoText
        {
            get => _displayInfoText;
            set => Set(ref _displayInfoText, value);
        }


        public ICommand ShowGpuListCommand { get; }
        public event EventHandler RequestShowGpuList;

        public HomeViewModel()
        {
            ShowGpuListCommand = new RelayCommand(OnShowGpuList);
        }
        public void LoadInfo()
        {
            // 壁纸
            try
            {
                Wallpaper = new BitmapImage(new Uri(Explorer.Wallpaper));
            }
            catch { }
            // 处理器
            CPUName = DeviceInfo.GetCPUName();
            // 内存
            RAMInfo = Convert.ToSingle(
                (double)DeviceInfo.GetRAM() / (1024 * 1024 * 1024)
            ).ToString("F1") + " GB";
            // 显卡
            string[] gpuTmp = DeviceInfo.GetGPU();
            GPUList = string.Join("\n", gpuTmp);
            GPUName = ResolveGpuDisplayName(gpuTmp);
            // Windows 信息
            WindowsVersion = DeviceInfo.GetWindowsVersion();
            WindowsBrandName = DeviceInfo.GetWindowsBrandingName();
            // 库版本
            LibVersion =$"{App.AppVersion}  |  项目维护：卡茜娅·元子喵";
            // 主板
            BaseBoardInfo = DeviceInfo.GetBIOSInfo(0);
            // 屏幕
            DisplayInfoText = FormatDisplayInfo();
        }

        private string ResolveGpuDisplayName(string[] gpuTmp)
        {
            if (gpuTmp == null || gpuTmp.Length == 0) return "未知";
            string firstGPU = gpuTmp[0];
            if (gpuTmp.Length >= 2)
            {
                foreach (string gpu in gpuTmp)
                {
                    if (gpu.Contains("NVIDIA")) { firstGPU = gpu; break; }
                    else if (gpu.Contains("Radeon") && gpu.Contains("RX")) { firstGPU = gpu; break; }
                    else if (gpu.Contains("Arc")) { firstGPU = gpu; break; }
                    else if (gpu.Contains("Radeon")) { firstGPU = gpu; break; }
                    else if (gpu.Contains("Iris")) { firstGPU = gpu; break; }
                    else if (gpu.Contains("UHD") || gpu.Contains("HD")) { firstGPU = gpu; break; }
                }
                return $"{firstGPU}，点此展开列表";
            }
            return firstGPU;
        }
        /// <summary>
        /// 格式化屏幕分辨率信息
        /// </summary>
        private string FormatDisplayInfo()
        {
            int x = DeviceInfo.GetDisplayInfo(0);
            int y = DeviceInfo.GetDisplayInfo(1);
            int r = DeviceInfo.GetDisplayInfo(2);
            string tier;
            if (y >= 2160)
                tier = "UHD";
            else if (y >= 1440)
                tier = ((double)x / y) <= 16.0 / 9.0 ? "QHD" : "WQHD";
            else if (y >= 1080)
                tier = x <= 1920 ? "FHD" : "FHD+";
            else if (y >= 720)
                tier = "HD";
            else
                tier = "SD";
            return $"({tier}) {x} × {y} @ {r}Hz";
        }
        private void OnShowGpuList()
        {
            RequestShowGpuList?.Invoke(this, EventArgs.Empty);
        }
    }
}
