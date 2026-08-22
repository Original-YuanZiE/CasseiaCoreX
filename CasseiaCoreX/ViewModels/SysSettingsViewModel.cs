using CasseiaCoreX.Model;
using CasseiaCoreX.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Management.Update;

namespace CasseiaCoreX.ViewModels
{
    public class SysSettingsViewModel : ViewModelBase
    {
        // 隐藏快捷方式角标
        private bool _isShortcutArrowHidden;
        public bool IsShortcutArrowHidden
        {
            get => _isShortcutArrowHidden;
            set
            {
                if (Set(ref _isShortcutArrowHidden, value))
                {
                    Explorer.ShortcutArrowHidden = value;
                }
            }
        }

        // 任务栏显秒
        private bool _isTaskBarShowSec;
        public bool IsTaskBarShowSec
        {
            get => _isTaskBarShowSec;
            set
            {
                if (Set(ref _isTaskBarShowSec, value))
                {
                    Explorer.ShowSecOnTaskBar = value;
                }
            }
        }

        // CMD 自动执行
        private string _cmdAutoRun;
        public string CMDAutoRun
        {
            get => _cmdAutoRun;
            set
            {
                if (Set(ref _cmdAutoRun, value))
                {
                    SystemSettings.CMDAutoRun = value;
                }
            }
        }

        // 长路径支持
        private bool _longPathSupport;
        public bool LongPathSupport
        {
            get => _longPathSupport;
            set
            {
                if(Set(ref _longPathSupport, value))
                {
                    SystemSettings.LongPathsEnabled = value;
                }
            }
        }

        // LogonUI 详细信息
        private bool _logonUIInfo;
        public bool LogonUIInfo
        {
            get => _logonUIInfo;
            set
            {
                if(Set(ref _logonUIInfo, value))
                {
                    SystemSettings.LogonUIVerboseInfo = value;
                }
            }
        }

        // UAC 设置
        private int _uacSettings;
        public int UACSettings
        {
            get => _uacSettings;
            set
            {
                if(Set(ref _uacSettings, value))
                {
                    SystemSettings.UACBehavior = value;
                }
            }
        }

        // 禁用 Defender
        private bool _disableDefender;
        public bool DisableDefender
        {
            get => _disableDefender;
            set
            {
                if(Set(ref _disableDefender, value))
                {
                    SystemSettings.SwitchDefender = value;
                }
            }
        }

        // 禁用 WindowsUpdate
        private bool _disableUpdate;
        public bool DisableUpdate
        {
            get => _disableUpdate;
            set
            {
                if(Set(ref _disableUpdate, value))
                {
                    Model.SystemSettings.WindowsUpdate WU = new Model.SystemSettings.WindowsUpdate();
                    if (value)
                    {
                        WU.WUServer = "127.0.0.1";
                        WU.WUStatusServer = "127.0.0.1";
                        WU.UpdateServiceUrlAlternate = "127.0.0.1";
                        WU.IsEnable = true;
                    }
                    else
                    {
                        WU.WUServer = null;
                        WU.WUStatusServer = null;
                        WU.UpdateServiceUrlAlternate = null;
                        WU.IsEnable = false;
                    }
                    WU.Close();
                }
            }
        }

        // 最大暂停更新时间
        private int _maxDelayTime;
        public int MaxDelayTime
        {
            get => _maxDelayTime;
            set
            {
                if(Set(ref _maxDelayTime, value))
                {
                    Model.SystemSettings.WindowsUpdate WU = new Model.SystemSettings.WindowsUpdate();
                    WU.MaxAllowDelayDays = value;
                    WU.Close();
                }
            }
        }

        // 强制启用圆角与 Mica
        private bool _forceEffectMode;
        public bool ForceEffectMode
        {
            get => _forceEffectMode;
            set
            {
                if(Set(ref _forceEffectMode, value))
                {
                    SystemSettings.ForceEffectMode = value;
                }
            }
        }

        // 解锁平板模式任务栏
        public ICommand UnLockTabletModeCommand;
        public event EventHandler UnLockTabletModeEvent;

        // 重启资源管理器
        public ICommand RestartExplorer;
        public event EventHandler RestartExplorerEvent;

        // CMD 自动执行
        public ICommand CMDAutoRunCommand;
        public event EventHandler CMDAutoRunEvent;

        // UAC 设置
        public ICommand UACSettingsCommand;
        public event EventHandler UACSettingsEvent;

        // 设置最大暂停更新时间
        public ICommand MaxDelayUpdateTimeCommand;
        public event EventHandler MaxDelayUpdateTimeEvent;

        // 清除密钥
        public ICommand ClearKeysCommand;
        public event EventHandler ClearKeysEvent;

        // MAS 激活
        public ICommand MASCommand;
        public event EventHandler MASEvent;

        // 数字激活
        public ICommand HWIDCommand;
        public event EventHandler HWIDEvent;

        // Home 转 Pro
        public ICommand Home2ProCommand;
        public event EventHandler Home2ProEvent;

        // 备份密钥
        public ICommand BackupKeysCommand;
        public event EventHandler BackupKeysEvent;

        // 设置 OEM 信息
        public ICommand SetOEMInfoCommand;
        public event EventHandler SetOEMInfoEvent;

        public SysSettingsViewModel()
        {
            UnLockTabletModeCommand = new RelayCommand(() =>
            {
                UnLockTabletModeEvent?.Invoke(this, EventArgs.Empty);
            });

            RestartExplorer = new RelayCommand(() =>
            {
                RestartExplorerEvent?.Invoke(this, EventArgs.Empty);
            });

            CMDAutoRunCommand = new RelayCommand(() =>
            {
                CMDAutoRunEvent?.Invoke(this, EventArgs.Empty);
            });

            UACSettingsCommand = new RelayCommand(() =>
            {
                UACSettingsEvent?.Invoke(this, EventArgs.Empty);
            });

            MaxDelayUpdateTimeCommand = new RelayCommand(() =>
            {
                MaxDelayUpdateTimeEvent?.Invoke(this, EventArgs.Empty);
            });

            ClearKeysCommand = new RelayCommand(() =>
            {
                ClearKeysEvent?.Invoke(this, EventArgs.Empty);
            });

            HWIDCommand = new RelayCommand(() =>
            {
                HWIDEvent?.Invoke(this, EventArgs.Empty);
            });

            Home2ProCommand = new RelayCommand(() =>
            {
                Home2ProEvent?.Invoke(this, EventArgs.Empty);
            });

            BackupKeysCommand = new RelayCommand(() =>
            {
                BackupKeysEvent?.Invoke(this, EventArgs.Empty);
            });

            SetOEMInfoCommand = new RelayCommand(() =>
            {
                SetOEMInfoEvent?.Invoke(this, EventArgs.Empty);
            });

            MASCommand = new RelayCommand(() =>
            {
                MASEvent?.Invoke(this, EventArgs.Empty);
            });
        }

        public void LoadInfo()
        {
            Set(ref _isShortcutArrowHidden, Explorer.ShortcutArrowHidden, nameof(IsShortcutArrowHidden));
            Set(ref _isTaskBarShowSec, Explorer.ShowSecOnTaskBar, nameof(IsTaskBarShowSec));
            Set(ref _cmdAutoRun, SystemSettings.CMDAutoRun, nameof(CMDAutoRun));
            Set(ref _longPathSupport, SystemSettings.LongPathsEnabled, nameof(LongPathSupport));
            Set(ref _logonUIInfo, SystemSettings.LogonUIVerboseInfo, nameof(LogonUIInfo));
            Set(ref _uacSettings, SystemSettings.UACBehavior, nameof(UACSettings));
            Set(ref _disableDefender, SystemSettings.SwitchDefender, nameof(DisableDefender));
            SystemSettings.WindowsUpdate WU = new SystemSettings.WindowsUpdate();
            Set(ref _maxDelayTime, WU.MaxAllowDelayDays, nameof(MaxDelayTime));
            Set(ref _forceEffectMode, SystemSettings.ForceEffectMode, nameof(ForceEffectMode));
            WU.Close();
        }

        
    }
}
