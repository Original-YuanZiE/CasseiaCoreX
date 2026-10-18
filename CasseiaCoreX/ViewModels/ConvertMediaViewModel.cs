using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CasseiaCoreX.ViewModels
{
    public class ConvertMediaViewModel : ViewModelBase
    {
        public static readonly string[] CommonMediaFormats =
        {
            "mp4",
            "mkv",
            "mov",
            "avi",
            "webm",
            "m4v",
            "wmv",
            "mpg",
            "mpeg",
            "mp3",
            "m4a",
            "aac",
            "wav",
            "flac",
            "ogg",
            "opus",
            "wma"
        };

        private Microsoft.UI.Xaml.XamlRoot _xamlRoot;
        public Microsoft.UI.Xaml.XamlRoot XamlRoot
        {
            get => _xamlRoot;
            set
            {
                Set(ref _xamlRoot, value);
            }
        }

        private string _selFilesText = "";
        public string SelFilesText
        {
            get => _selFilesText;
            set
            {
                Set(ref _selFilesText, value);
            }
        }

        private ComboBoxItem _selEncodeType;
        public ComboBoxItem SelEncodeType
        {
            get => _selEncodeType;
            set
            {
                Set(ref _selEncodeType, value);
            }
        }

        private string _targetFolder;
        public string TargetFolder
        {
            get => _targetFolder;
            set
            {
                Set(ref _targetFolder, value);
            }
        }

        public ObservableCollection<ComboBoxItem> FileTypeComboItems { get; set; } = new ObservableCollection<ComboBoxItem>();

        public ICommand SelMediaCmd;
        public event EventHandler SelMediaEvt;

        public ICommand SelFolderCmd;
        public event EventHandler SelFolderEvt;

        public ConvertMediaViewModel()
        {
            SelMediaCmd = new RelayCommand(() =>
            {
                SelMediaEvt?.Invoke(this, EventArgs.Empty);
            });

            SelFolderCmd = new RelayCommand(() =>
            {
                SelFolderEvt?.Invoke(this, EventArgs.Empty);
            });

            foreach (var ext in CommonMediaFormats)
            {
                FileTypeComboItems.Add(new ComboBoxItem
                {
                    Content = $".{ext}"
                });
            }
        }
    }
}