using Pascal.Wallet.Connector;
using System.Windows;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class MainViewModel : ViewModelBase, IConnectorHolder
    {
        private object _mainContent;
        private ICommand _settingsCommand;
        private ICommand _dataOperationCommand;
        private ICommand _multiOperationCommand;
        private ConnectionViewModel _connectionViewModel;
        private DataOperationViewModel _dataOperationViewModel;
        private MultiOperationViewModel _multiOperationViewModel;
        private PascalConnector _connector;

        public MainViewModel()
        {
            MainContent = _connectionViewModel = new ConnectionViewModel(this);
        }

        public ICommand SettingsCommand => _settingsCommand ??= new RelayCommand(() => MainContent = _connectionViewModel ??= new ConnectionViewModel(this));
        public ICommand DataOperationCommand => _dataOperationCommand ??= new RelayCommandAsync(async () =>
        {
            var shouldInitialize = _dataOperationViewModel == null;
            MainContent = _dataOperationViewModel ??= new DataOperationViewModel(this);
            if (shouldInitialize)
            {
                await _dataOperationViewModel.InitializeAsync();
            }
        });
        public ICommand MultiOperationCommand => _multiOperationCommand ??= new RelayCommand(() => MainContent = _multiOperationViewModel ??= new MultiOperationViewModel(this));
        public ICommand CloseCommand => new RelayCommand(parameter => Application.Current.Shutdown());

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
        public PascalConnector Connector
        {
            get { return _connector; }
            set
            {
                _connector = value;
                _dataOperationViewModel = null;
                _multiOperationViewModel = null;
            }
        }
    }
}
