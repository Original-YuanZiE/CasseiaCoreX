using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CasseiaCoreX.ViewModels
{
    public class ToolsViewModel : ViewModelBase
    {

        // 备份驱动
        public ICommand BackupDriversCommand;
        public event EventHandler BackupDriversEvent;

        // 导入驱动
        public ICommand ImportDriversCommand;
        public event EventHandler ImportDriversEvent;

        // 图片格式转换
        public ICommand ConvertImageFormatCommand;
        public event EventHandler ConvertImageFormatEvent;

        // 媒体格式转换
        public ICommand ConvertMediaFormatCommand;
        public event EventHandler ConvertMediaFormatEvent;

        // 补全 Codecs
        public ICommand CompleteWICCodecsCommand;
        public event EventHandler CompleteWICCodecsEvent;

        public ToolsViewModel()
        {
            BackupDriversCommand = new RelayCommand(() =>
            {
                BackupDriversEvent?.Invoke(this, EventArgs.Empty);
            });
            ImportDriversCommand = new RelayCommand(() =>
            {
                ImportDriversEvent?.Invoke(this, EventArgs.Empty);
            });
            ConvertImageFormatCommand = new RelayCommand(() =>
            {
                ConvertImageFormatEvent?.Invoke(this, EventArgs.Empty);
            });
            ConvertMediaFormatCommand = new RelayCommand(() =>
            {
                ConvertMediaFormatEvent?.Invoke(this, EventArgs.Empty);
            });
            CompleteWICCodecsCommand = new RelayCommand(() =>
            {
                CompleteWICCodecsEvent?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
