using Pascal.Wallet.Connector.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class DataOperationViewModel: ViewModelBase
    {
        private IConnectorHolder _holder;
        private InfoMessageViewModel _infoMessage;
        private Account _senderAccount;
        private Account _signerAccount;
        private uint _receiverAccount;
        private decimal _fee;
        private string _identifier;
        private string _message;
        private List<Account> _accounts;
        private List<Account> _accountsWithPasc;

        public DataOperationViewModel(IConnectorHolder connectorHolder)
        {
            _holder = connectorHolder;
            ReceiverAccount = 834853;
            GenerateGuid();

            //this should be created on the UI thread
            var errorInfo = new InfoMessageViewModel("Failed to load accounts! Check if Pascal Wallet is open and it accepts connections.", () => InfoMessage = null, true);

            var accountsLoaded = false;
            var timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            timer.Start();
            void TimerTick(object sender, EventArgs e)
            {
                timer.Stop();
                timer.Tick -= TimerTick;
                if (!accountsLoaded)
                {
                    InfoMessage = new InfoMessageViewModel("Loading accounts...", null);
                }
            }

            _holder.Connector.GetWalletAccountsAsync(max: 1000).ContinueWith(task => {
                accountsLoaded = true;
                if (task.Result.Result != null)
                {
                    Accounts = task.Result.Result.OrderBy(n => n.AccountNumber).ToList();
                    InfoMessage = null;
                }
                else
                {
                    InfoMessage = errorInfo;
                }
            }).ConfigureAwait(false);

            SendCommand = new RelayCommandAsync(SendDataOperation, parameter => CanSend());
            ClearCommand = new RelayCommand(Clear);
            GenerateGuidCommand = new RelayCommand(GenerateGuid);
        }

        public async Task SendDataOperation()
        {
            var sendingDataResponse = await _holder.Connector.SendDataAsync(SenderAccount.AccountNumber, ReceiverAccount, Identifier, SignerAccount?.AccountNumber, DataType.ChatMessage, 0, 0, Fee, Message);
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
        public ICommand ClearCommand { get; private set; }
        public ICommand GenerateGuidCommand { get; private set; }

        private void GenerateGuid()
        {
            Identifier = Guid.NewGuid().ToString().ToUpper();
        }

        private void Clear()
        {
            ReceiverAccount = 834853;
            SignerAccount = null;
            SenderAccount = null;
            Message = null;
            Fee = 0;
        }

        private bool CanSend()
        {
            return SenderAccount != null;
        }
    }
}
