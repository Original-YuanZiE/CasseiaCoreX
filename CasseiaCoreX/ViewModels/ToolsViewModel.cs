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
        }
    }
}
