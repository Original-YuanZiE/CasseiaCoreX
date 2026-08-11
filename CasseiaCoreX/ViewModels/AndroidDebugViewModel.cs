using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CasseiaCoreX.ViewModels
{
    public class AndroidDebugViewModel : ViewModelBase
    {

        // 当前已选择的设备
        private string _selectedDevice;
        public string SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                Set(ref _selectedDevice, value);
            }
        }

        // 结束 ADB 进程
        public ICommand KillAdbCommand;
        public event EventHandler KillAdbEvent;

        // 更新 ADB
        public ICommand UpdateAdbCommand;
        public event EventHandler UpdateAdbEvent;

        // 选择设备
        public ICommand SelectDeviceCommand;
        public event EventHandler SelectDeviceEvent;

        // 屏幕截图
        public ICommand ScreenshotCommand;
        public event EventHandler ScreenshotEvent;

        // 使用配对码
        public ICommand PairDeviceCommand;
        public event EventHandler PairDeviceEvent;

        // 直接连接
        public ICommand ConnectDeviceCommand;
        public event EventHandler ConnectDeviceEvent;

        // 安装应用
        public ICommand InstallAppCommand;
        public event EventHandler InstallAppEvent;

        // 管理应用
        public ICommand ManageAppCommand;
        public event EventHandler ManageAppEvent;

        public AndroidDebugViewModel()
        {
            KillAdbCommand = new RelayCommand(() =>
            {
                KillAdbEvent?.Invoke(this, EventArgs.Empty);
            });

            UpdateAdbCommand = new RelayCommand(() =>
            {
                UpdateAdbEvent?.Invoke(this, EventArgs.Empty);
            });

            SelectDeviceCommand = new RelayCommand(() =>
            {
                SelectDeviceEvent?.Invoke(this, EventArgs.Empty);
            });

            ScreenshotCommand = new RelayCommand(() =>
            {
                ScreenshotEvent?.Invoke(this, EventArgs.Empty);
            });

            PairDeviceCommand = new RelayCommand(() =>
            {
                PairDeviceEvent?.Invoke(this, EventArgs.Empty);
            });

            ConnectDeviceCommand = new RelayCommand(() =>
            {
                ConnectDeviceEvent?.Invoke(this, EventArgs.Empty);
            });

            InstallAppCommand = new RelayCommand(() =>
            {
                InstallAppEvent?.Invoke(this, EventArgs.Empty);
            });

            ManageAppCommand = new RelayCommand(() =>
            {
                ManageAppEvent?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
