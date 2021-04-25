using Pascal.Wallet.Connector;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class ConnectionViewModel: ViewModelBase, IPasswordsHolder
    {
        private IConnectorHolder _connectorHolder;
        private string _address;
        private uint _port;
        private uint _defaultReceiver;
        private string _passwords;
        private bool _passwordsChanged;
        private int _messageRefreshingInterval;

        public ConnectionViewModel(IConnectorHolder connectorHolder)
        {
            _connectorHolder = connectorHolder;

            Address = Properties.Settings.Default.WalletAddress;
            Port = Properties.Settings.Default.WalletPort;
            DefaultReceiver = Properties.Settings.Default.DefaultReceiver;
            MessageRefreshingInterval = Properties.Settings.Default.MessageRefreshingInterval;
            SaveCommand = new RelayCommand(SaveSettings, parameter => CanSaveSettings());
            _connectorHolder.Connector = new PascalConnector(Address, Port);
        }

        public ICommand SaveCommand { get; private set; }

        public string Address
        {
            get { return _address; }
            set
            {
                if(_address != value)
                {
                    _address = value;
                    OnPropertyChanged(nameof(Address));
                }
            }
        }

        public uint Port
        {
            get { return _port; }
            set
            {
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }

        public uint DefaultReceiver
        {
            get { return _defaultReceiver; }
            set
            {
                _defaultReceiver = value;
                OnPropertyChanged(nameof(DefaultReceiver));
            }
        }

        public int MessageRefreshingInterval
        {
            get { return _messageRefreshingInterval; }
            set
            {
                _messageRefreshingInterval = value;
                OnPropertyChanged(nameof(MessageRefreshingInterval));
            }
        }

        public string Passwords
        {
            get { return _passwords; }
            set
            {
                if (_passwords != value)
                {
                    _passwords = value;
                    OnPropertyChanged(nameof(Passwords));
                    _passwordsChanged = true;
                }
            }
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.WalletAddress = Address;
            Properties.Settings.Default.WalletPort = Port;
            Properties.Settings.Default.DefaultReceiver = DefaultReceiver;
            Properties.Settings.Default.MessageRefreshingInterval = MessageRefreshingInterval;
            Properties.Settings.Default.Save();
            _connectorHolder.Connector = new PascalConnector(Address, Port);
            _passwordsChanged = false;
        }

        private bool CanSaveSettings()
        {
            return !string.IsNullOrEmpty(Address) && (Address != Properties.Settings.Default.WalletAddress || Port != Properties.Settings.Default.WalletPort || MessageRefreshingInterval != Properties.Settings.Default.MessageRefreshingInterval
                || Properties.Settings.Default.DefaultReceiver != DefaultReceiver || _passwordsChanged);
        }
    }
}
