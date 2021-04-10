using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class InfoMessageViewModel: ViewModelBase
    {
        private string _message;
        private string _button1Text;
        private string _button2Text;
        private string _button3Text;
        private Visibility _visibility1;
        private Visibility _visibility2;
        private Visibility _visibility3;
        private bool _isDefault1;
        private bool _isDefault2;
        private bool _isDefault3;
        private bool _isCancel1;
        private bool _isCancel2;
        private bool _isCancel3;
        private ICommand _button1Command;
        private ICommand _button2Command;
        private ICommand _button3Command;
        private Brush _backgroundBrush;

        /// <summary>Creates infoMessageViewModel instance</summary>
        /// <param name="message">Error message string</param>
        /// <param name="closeAction">Command that will be performed during message info window closing operation</param>
        public InfoMessageViewModel(string message, Action closeAction) : this(message, closeAction, false) { }

        public InfoMessageViewModel(string message, Action closeAction, bool isError) : this(message, "Ok", closeAction, null, null, isError) { }

        /// <summary>Creates infoMessageViewModel instance</summary>
        /// <param name="message">Error message string</param>
        /// <param name="isError">Determines if background brush will be red</param>
        public InfoMessageViewModel(string message, string button1Text, Action button1Action, string button2Text, Action button2Action, bool isError)
        {
            Message = message;
            if (button1Action != null)
            {
                Button1Command = new RelayCommand(button1Action);
            }
            if (button2Action != null)
            {
                Button2Command = new RelayCommand(button2Action);
            }

            Button1Text = button1Text;
            Button2Text = button2Text;
            Button3Text = null;
            Visibility1 = button1Action != null ? Visibility.Visible : Visibility.Collapsed;
            Visibility2 = button2Text != null ? Visibility.Visible : Visibility.Collapsed;
            Visibility3 = Visibility.Collapsed;
            IsDefault1 = true;
            IsDefault2 = false;
            IsDefault3 = false;
            IsCancel1 = true;
            IsCancel2 = false;
            IsCancel3 = false;

            BackgroundBrush = new SolidColorBrush(
                isError ? new Color { A = 128, R = 205, G = 92, B = 92 } : new Color { A = 128, R = 80, G = 80, B = 112 });
        }

        /// <summary>Creates infoMessageViewModel instance</summary>
        /// <param name="message">Error message string</param>
        /// <param name="isError">Determines if background brush will be red</param>
        public InfoMessageViewModel(string message, string button1Text, Action button1Action, string button2Text, Action button2Action, string button3Text, Action button3Action, bool isError)
        {
            Message = message;
            if (button1Action != null)
            {
                Button1Command = new RelayCommand(button1Action);
            }
            if (button2Action != null)
            {
                Button2Command = new RelayCommand(button2Action);
            }
            if (button3Action != null)
            {
                Button3Command = new RelayCommand(button3Action);
            }

            Button1Text = button1Text;
            Button2Text = button2Text;
            Button3Text = button3Text;
            Visibility1 = Visibility.Visible;
            Visibility2 = button2Text != null ? Visibility.Visible : Visibility.Collapsed;
            Visibility3 = button3Text != null ? Visibility.Visible : Visibility.Collapsed;
            IsDefault1 = true;
            IsDefault2 = false;
            IsDefault3 = false;
            IsCancel1 = true;
            IsCancel2 = false;
            IsCancel3 = false;

            BackgroundBrush = new SolidColorBrush(
                isError ? new Color { A = 128, R = 205, G = 92, B = 92 } : new Color { A = 128, R = 80, G = 80, B = 112 });
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        public string Button1Text
        {
            get { return _button1Text; }
            set
            {
                if (_button1Text != value)
                {
                    _button1Text = value;
                    OnPropertyChanged(nameof(Button1Text));
                }
            }
        }

        public string Button2Text
        {
            get { return _button2Text; }
            set
            {
                if (_button2Text != value)
                {
                    _button2Text = value;
                    OnPropertyChanged(nameof(Button2Text));
                }
            }
        }

        public string Button3Text
        {
            get { return _button3Text; }
            set
            {
                if (_button3Text != value)
                {
                    _button3Text = value;
                    OnPropertyChanged(nameof(Button3Text));
                }
            }
        }

        public Brush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set
            {
                if (_backgroundBrush != value)
                {
                    _backgroundBrush = value;
                    OnPropertyChanged(nameof(BackgroundBrush));
                }
            }
        }

        public Visibility Visibility1
        {
            get { return _visibility1; }
            set
            {
                if (_visibility1 != value)
                {
                    _visibility1 = value;
                    OnPropertyChanged(nameof(Visibility1));
                }
            }
        }

        public Visibility Visibility2
        {
            get { return _visibility2; }
            set
            {
                if (_visibility2 != value)
                {
                    _visibility2 = value;
                    OnPropertyChanged(nameof(Visibility2));
                }
            }
        }

        public Visibility Visibility3
        {
            get { return _visibility3; }
            set
            {
                if (_visibility3 != value)
                {
                    _visibility3 = value;
                    OnPropertyChanged(nameof(Visibility3));
                }
            }
        }

        public bool IsDefault1
        {
            get { return _isDefault1; }
            set
            {
                if (_isDefault1 != value)
                {
                    _isDefault1 = value;
                    OnPropertyChanged(nameof(IsDefault1));
                }
            }
        }

        public bool IsDefault2
        {
            get { return _isDefault2; }
            set
            {
                if (_isDefault2 != value)
                {
                    _isDefault2 = value;
                    OnPropertyChanged(nameof(IsDefault2));
                }
            }
        }

        public bool IsDefault3
        {
            get { return _isDefault3; }
            set
            {
                if (_isDefault3 != value)
                {
                    _isDefault3 = value;
                    OnPropertyChanged(nameof(IsDefault3));
                }
            }
        }

        public bool IsCancel1
        {
            get { return _isCancel1; }
            set
            {
                if (_isCancel1 != value)
                {
                    _isCancel1 = value;
                    OnPropertyChanged(nameof(IsCancel1));
                }
            }
        }

        public bool IsCancel2
        {
            get { return _isCancel2; }
            set
            {
                if (_isCancel2 != value)
                {
                    _isCancel2 = value;
                    OnPropertyChanged(nameof(IsCancel2));
                }
            }
        }

        public bool IsCancel3
        {
            get { return _isCancel3; }
            set
            {
                if (_isCancel3 != value)
                {
                    _isCancel3 = value;
                    OnPropertyChanged(nameof(IsCancel3));
                }
            }
        }

        public ICommand Button1Command
        {
            get { return _button1Command; }
            set
            {
                if (_button1Command != value)
                {
                    _button1Command = value;
                    OnPropertyChanged(nameof(Button1Command));
                }
            }
        }

        public ICommand Button2Command
        {
            get { return _button2Command; }
            set
            {
                if (_button2Command != value)
                {
                    _button2Command = value;
                    OnPropertyChanged(nameof(Button2Command));
                }
            }
        }

        public ICommand Button3Command
        {
            get { return _button3Command; }
            set
            {
                if (_button3Command != value)
                {
                    _button3Command = value;
                    OnPropertyChanged(nameof(Button3Command));
                }
            }
        }
    }
}
