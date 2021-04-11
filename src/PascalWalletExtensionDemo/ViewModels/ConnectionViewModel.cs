using Pascal.Wallet.Connector;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class ConnectionViewModel: ViewModelBase
    {
        private IConnectorHolder _connectorHolder;
        private string _address;
        private uint _port;

        public ConnectionViewModel(IConnectorHolder connectorHolder)
        {
            _connectorHolder = connectorHolder;

            Address = Properties.Settings.Default.WalletAddress;
            Port = Properties.Settings.Default.WalletPort;
            ReconnectCommand = new RelayCommand(ReconnectSettings, parameter => CanReconnectSettings());
            _connectorHolder.Connector = new PascalConnector(Address, Port);
        }

        public ICommand ReconnectCommand { get; private set; }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public uint Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        private void ReconnectSettings()
        {
            Properties.Settings.Default.WalletAddress = Address;
            Properties.Settings.Default.WalletPort = Port;
            Properties.Settings.Default.Save();
            _connectorHolder.Connector = new PascalConnector(Address, Port);
        }

        private bool CanReconnectSettings()
        {
            return !string.IsNullOrEmpty(Address) && (Address != Properties.Settings.Default.WalletAddress || Port != Properties.Settings.Default.WalletPort);
        }
    }
}
