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

            Address = "127.0.0.1";
            Port = 4003;
            SaveCommand = new RelayCommand(SaveSettings, parameter => CanSaveSettings());
            SaveSettings();
        }

        public ICommand SaveCommand { get; private set; }

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

        private void SaveSettings()
        {
            _connectorHolder.Connector = new PascalConnector(Address, Port);
        }

        private bool CanSaveSettings()
        {
            return !string.IsNullOrEmpty(Address);
        }
    }
}
