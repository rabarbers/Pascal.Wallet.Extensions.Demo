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
        private Account _senderAccount;
        private Account _signerAccount;
        private uint _receiverAccount;
        private decimal _fee;
        private string _identifier;
        private string _message;
        private List<Account> _accounts;
        private List<Account> _accountsWithPasc;

        public DataOperationViewModel(PascalConnector connector)
        {
            _connector = connector;
            ReceiverAccount = 834853;
            GenerateGuid();

            InfoMessage = new InfoMessageViewModel("Connecting to Pascal Wallet...", null);

            //this should be created on the UI thread
            var error = new InfoMessageViewModel("Failed to connect to Pascal Wallet", () => Application.Current.Shutdown(), true);

            _connector.GetWalletAccountsAsync(max: 300).ContinueWith(task => {
                if(task.Result.Result != null)
                {
                    Accounts = task.Result.Result.OrderBy(n => n.AccountNumber).ToList();
                    InfoMessage = null;
                }
                else
                {
                    InfoMessage = error;
                }
            }).ConfigureAwait(false);

            SendCommand = new RelayCommandAsync(SendDataOperation, parameter => CanSend());
            GenerateGuidCommand = new RelayCommand(GenerateGuid);
        }

        public async Task SendDataOperation()
        {
            var sendingDataResponse = await _connector.SendDataAsync(SenderAccount.AccountNumber, ReceiverAccount, Identifier, SignerAccount?.AccountNumber, DataType.ChatMessage, 0, 0, Fee, Message);
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

        public List<Account> AccountsWithPasc
        {
            get { return _accountsWithPasc; }
            set
            {
                _accountsWithPasc = value;
                OnPropertyChanged(nameof(AccountsWithPasc));
            }
        }

        public List<Account> Accounts
        {
            get { return _accounts; }
            set
            {
                _accounts = value;
                OnPropertyChanged(nameof(Accounts));
            }
        }

        public Account SenderAccount
        {
            get { return _senderAccount; }
            set
            {
                if(value != null)
                {
                    AccountsWithPasc = Accounts.Where(n => n.EncodedPublicKey == value.EncodedPublicKey).ToList();
                }
                else
                {
                    AccountsWithPasc = new List<Account>();
                }

                _senderAccount = value;
                OnPropertyChanged(nameof(SenderAccount));
            }
        }

        public Account SignerAccount
        {
            get { return _signerAccount; }
            set
            {
                _signerAccount = value;
                OnPropertyChanged(nameof(SignerAccount));
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
        public ICommand GenerateGuidCommand { get; private set; }

        private void GenerateGuid()
        {
            Identifier = Guid.NewGuid().ToString().ToUpper();
        }

        private bool CanSend()
        {
            return SenderAccount != null;
        }
    }
}
