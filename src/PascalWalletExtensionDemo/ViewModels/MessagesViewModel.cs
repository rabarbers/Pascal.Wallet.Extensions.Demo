using Pascal.Wallet.Connector;
using Pascal.Wallet.Connector.DTO;
using PascalWalletExtensionDemo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class MessagesViewModel: ViewModelBase, IErrorMessageHolder
    {
        private readonly IConnectorHolder _holder;
        private InfoMessageViewModel _infoMessage;
        private bool _isBusy;
        private List<Account> _accounts;
        private List<Message> _messages;

        public MessagesViewModel(IConnectorHolder connectorHolder)
        {
            _holder = connectorHolder;

            RefreshCommand = new RelayCommandAsync(InitializeAsync, parameter => CanRefresh());
        }

        public ICommand RefreshCommand { get; private set; }

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

        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
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

        public List<Message> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public async Task InitializeAsync()
        {
            _isBusy = true;

            ViewModelHelper.SetErrorMessage(this, "Loading accounts...");
            var accountsResponse = await _holder.Connector.GetWalletAccountsAsync(max: 500);
            if (accountsResponse.Result != null)
            {
                Accounts = accountsResponse.Result.OrderBy(n => n.AccountNumber).ToList();
                InfoMessage = null;
            }
            else
            {
                InfoMessage = new InfoMessageViewModel("Failed to load accounts! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
            }

            var messages = new List<Message>();

            ViewModelHelper.SetErrorMessage(this, "Loading messages...");
            foreach (var account in Accounts)
            {
                var receivedMessagesResponse = await _holder.Connector.FindDataOperationsAsync(receiverAccount: account.AccountNumber, max: int.MaxValue);
                if (receivedMessagesResponse.Result != null)
                {
                    foreach (var op in receivedMessagesResponse.Result)
                    {
                        //TODO use dictionary
                        var isContextUserSender = Accounts.Any(n => n.AccountNumber == op.Senders[0].AccountNumber);
                       
                        var message = new Message(op.Senders[0].AccountNumber, isContextUserSender, op.Receivers[0].AccountNumber, true, op.BlockNumber, op.Payload.FromHexString(), op.PayloadType);
                        messages.Add(message);
                    }
                    InfoMessage = null;
                }
                else
                {
                    InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                }

                var sentMessagesResponse = await _holder.Connector.FindDataOperationsAsync(senderAccount: account.AccountNumber, max: int.MaxValue);
                if (sentMessagesResponse.Result != null)
                {
                    foreach (var op in sentMessagesResponse.Result)
                    {
                        //TODO use dictionary
                        var isContextUserReceiver = Accounts.Any(n => n.AccountNumber == op.Receivers[0].AccountNumber);

                        var message = new Message(op.Senders[0].AccountNumber, true,  op.Receivers[0].AccountNumber, isContextUserReceiver, op.BlockNumber, op.Payload.FromHexString(), op.PayloadType);
                        messages.Add(message);
                    }
                    InfoMessage = null;
                }
                else
                {
                    InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                }
            }
            Messages = messages.OrderByDescending(n => n.BlockNumber).ToList();
            
            _isBusy = false;
        }

        private bool CanRefresh()
        {
            return !_isBusy;
        }
    }
}
