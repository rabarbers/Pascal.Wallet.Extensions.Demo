using Pascal.Wallet.Connector;
using Pascal.Wallet.Connector.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class DataOperationViewModel: ViewModelBase
    {
        private PascalConnector _connector;
        private InfoMessageViewModel _infoMessage;
        private uint _senderAccount;
        private uint _receiverAccount;
        private decimal _fee;
        private string _identifier;
        private string _message;
        private List<uint> _accounts;

        public DataOperationViewModel(PascalConnector connector)
        {
            _connector = connector;
            Identifier = Guid.NewGuid().ToString().ToUpper();
            ReceiverAccount = 834853;

            InfoMessage = new InfoMessageViewModel("Connecting to Pascal Wallet...", null);

            //this should be created on the UI thread
            var error = new InfoMessageViewModel("Failed to connect to Pascal Wallet", () => Application.Current.Shutdown(), true);

            _connector.GetWalletAccountsAsync(max: 100000).ContinueWith(task => {
                if(task.Result.Result != null)
                {
                    var accounts = task.Result.Result.Select(n => n.AccountNumber).OrderBy(n => n);
                    Accounts = new List<uint>(accounts);
                    InfoMessage = null;
                }
                else
                {
                    InfoMessage = error;
                }
            }).ConfigureAwait(false);

            SendCommand = new RelayCommandAsync(SendDataOperation);
        }

        public async Task SendDataOperation()
        {
            var sendingDataResponse = await _connector.SendDataAsync(SenderAccount, ReceiverAccount, Identifier, null, DataType.ChatMessage, 0, 0, Fee, Message);
            if (sendingDataResponse.Result != null)
            {
                InfoMessage = new InfoMessageViewModel($"DataOperation sent successfully.", () => InfoMessage = null);
            }
            else
            {
                InfoMessage = new InfoMessageViewModel(sendingDataResponse.Error.Message, () => InfoMessage = null, true);
            }
        }

        public InfoMessageViewModel InfoMessage
        {
            get { return _infoMessage; }
            set
            {
                if (_infoMessage != value)
                {
                    _infoMessage = value;
                    OnPropertyChanged(nameof(InfoMessage));
                }
            }
        }

        public List<uint> Accounts
        {
            get { return _accounts; }
            set
            {
                _accounts = value;
                OnPropertyChanged(nameof(Accounts));
            }
        }

        public uint SenderAccount
        {
            get { return _senderAccount; }
            set
            {
                _senderAccount = value;
                OnPropertyChanged(nameof(SenderAccount));
            }
        }

        public uint ReceiverAccount
        {
            get { return _receiverAccount; }
            set
            {
                _receiverAccount = value;
                OnPropertyChanged(nameof(ReceiverAccount));
            }
        }

        public decimal Fee
        {
            get { return _fee; }
            set
            {
                _fee = value;
                OnPropertyChanged(nameof(Fee));
            }
        }

        public string Identifier
        {
            get { return _identifier; }
            set
            {
                _identifier = value;
                OnPropertyChanged(nameof(Identifier));
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public ICommand SendCommand { get; private set; }
    }
}
