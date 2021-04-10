using Pascal.Wallet.Connector;
using System.Windows;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class MainViewModel: ViewModelBase
    {
        private object _mainContent;
        private PascalConnector _connector;

        public MainViewModel()
        {
            _connector = new PascalConnector(address: "127.0.0.1", port: 4003);

            MainContent = new DataOperationViewModel(_connector);

            DataOperationCommand = new RelayCommand(parameter => MainContent = new DataOperationViewModel(_connector));
            MultiOperationCommand = new RelayCommand(parameter => MainContent = new MultiOperationViewModel(_connector));
            CloseCommand = new RelayCommand(parameter => Application.Current.Shutdown());
        }

        public ICommand DataOperationCommand { get; private set; }
        public ICommand MultiOperationCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public object MainContent
        {
            get { return _mainContent; }
            set
            {
                if (_mainContent != value)
                {
                    _mainContent = value;
                    OnPropertyChanged(nameof(MainContent));
                }
            }
        }
    }
}
