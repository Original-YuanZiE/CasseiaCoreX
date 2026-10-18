using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;

namespace CasseiaCoreX.ViewModels
{


    public class ConvertImageViewModel : ViewModelBase
    {

        public static readonly string[] CommonImageFormats =
        {
            "png",
            "jpg",
            "jpeg",
            "bmp",
            "tiff",
            "tif",
            "webp",
            "gif",
            "apng",
            "avif",
            "ico",
            "heif",
            "heic",
            "hif"
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
                EnableQuality = value.Content?.ToString() is ".jpg" or ".jpeg" or ".webp" or ".avif";
            }
        }
        
        private int _selQualityItemIndex = 0;
        public int SelQualityItemIndex
        {
            get => _selQualityItemIndex;
            set
            {
                Set(ref _selQualityItemIndex, value);
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

        private bool _enableQuality = false;
        public bool EnableQuality
        {
            get => _enableQuality;
            set
            {
                Set(ref _enableQuality, value);
            }
        }

        public static Visibility BoolToVisibility(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public ObservableCollection<ComboBoxItem> FileTypeComboItems { get; set; } = new ObservableCollection<ComboBoxItem>();
        public ObservableCollection<ComboBoxItem> FileQualityComboItems { get; set; } = new ObservableCollection<ComboBoxItem>();


        public ICommand SelIamgeCmd;
        public event EventHandler SelIamgeEvt;

        public ICommand SelFolderCmd;
        public event EventHandler SelFolderEvt;

        public ConvertImageViewModel()
        {
            SelIamgeCmd = new RelayCommand(() =>
            {
                SelIamgeEvt?.Invoke(this, EventArgs.Empty);
            });
            SelFolderCmd = new RelayCommand(() =>
            {
                SelFolderEvt?.Invoke(this, EventArgs.Empty);
            });

            foreach (var ext in CommonImageFormats)
            {
                FileTypeComboItems.Add(new ComboBoxItem
                {
                    Content = $".{ext}"
                });
            }
        }

        
    }
}
