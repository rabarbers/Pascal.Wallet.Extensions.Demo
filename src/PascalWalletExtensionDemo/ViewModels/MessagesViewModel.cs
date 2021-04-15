using Pascal.Wallet.Connector;
using Pascal.Wallet.Connector.DTO;
using PascalWalletExtensionDemo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class MessagesViewModel: ViewModelBase
    {
        private readonly IConnectorHolder _holder;
        private InfoMessageViewModel _infoMessage;
        private bool _isBusy;
        private List<Account> _accounts;
        private List<Message> _messages;

        public MessagesViewModel(IConnectorHolder connectorHolder)
        {
            _holder = connectorHolder;

            RefreshCommand = new RelayCommandAsync(InitializeAsync);
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

            InfoMessage = new InfoMessageViewModel("Loading messages...", null);
            var accountsResponse = await _holder.Connector.GetWalletAccountsAsync(max: 500);
            if (accountsResponse.Result != null)
            {
                Accounts = accountsResponse.Result.OrderBy(n => n.AccountNumber).ToList();
            }
            else
            {
                InfoMessage = new InfoMessageViewModel("Failed to load accounts! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
            }

            var operations = new List<Operation>();
            foreach (var account in Accounts)
            {
                var receivedMessagesResponse = await _holder.Connector.FindDataOperationsAsync(receiverAccount: account.AccountNumber, max: int.MaxValue);
                if (receivedMessagesResponse.Result != null)
                {
                    operations.AddRange(receivedMessagesResponse.Result);
                }
                else
                {
                    InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                }

                var sentMessagesResponse = await _holder.Connector.FindDataOperationsAsync(senderAccount: account.AccountNumber, max: int.MaxValue);
                if (sentMessagesResponse.Result != null)
                {
                    operations.AddRange(sentMessagesResponse.Result);
                }
                else
                {
                    InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                }
            }

            var operationGroups = from operation in operations
                                  group operation by
                                  new
                                  {
                                      SenderAccount = operation.Senders[0].AccountNumber,
                                      ReceiverAccount = operation.Receivers[0].AccountNumber,
                                      operation.BlockNumber,
                                      DataType = operation.Senders[0].Data.Type,
                                      operation.Senders[0].Data.Id,
                                      operation.PayloadType
                                  }
                                  into operationsGroup
                                  select new
                                  {
                                      operationsGroup.Key,
                                      Items = operationsGroup
                                  };

            var messages = new List<Message>();
            foreach (var group in operationGroups)
            {
                //TODO use dictionary
                var isContextUserSender = Accounts.Any(n => n.AccountNumber == group.Key.SenderAccount);
                var isContextUserReceiver = Accounts.Any(n => n.AccountNumber == group.Key.ReceiverAccount);

                string payload = "";
                var isPublic = (group.Key.PayloadType & PayloadType.Public) == PayloadType.Public;
                var recipientKeyEncrypted = (group.Key.PayloadType & PayloadType.RecipientKeyEncrypted) == PayloadType.RecipientKeyEncrypted;
                var senderKeyEncrypted = (group.Key.PayloadType & PayloadType.SenderKeyEncrypted) == PayloadType.SenderKeyEncrypted;
                var passwordEncrypted = (group.Key.PayloadType & PayloadType.PasswordEncrypted) == PayloadType.PasswordEncrypted;
                var isAscii = (group.Key.PayloadType & PayloadType.AsciiFormatted) == PayloadType.AsciiFormatted;
                if (isPublic && isAscii || group.Key.PayloadType == PayloadType.NonDeterministic)
                {
                    foreach(var op in group.Items.OrderBy(n => n.Senders[0].Data.Sequence))
                    {
                        payload += op.Payload.FromHexString();
                    }
                }
                if (recipientKeyEncrypted)
                {
                    if (isContextUserReceiver)
                    {
                        foreach (var op in group.Items.OrderBy(n => n.Senders[0].Data.Sequence))
                        {
                            var decryptionResponse = await _holder.Connector.PayloadDecryptAsync(op.Payload);
                            if (decryptionResponse.Result != null)
                            {
                                if(decryptionResponse.Result.Result)
                                {
                                    payload += decryptionResponse.Result.UnencryptedPayload;
                                }
                                else
                                {
                                    payload += "Cannot decrypt, perhaps receiver's public key has changed.";
                                }
                            }
                            else
                            {
                                InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                            }
                        }
                    }
                    else
                    {
                        payload = "Cannot read. Encrypted with receiver's public key.";
                    }
                }
                if (senderKeyEncrypted)
                {
                    if (isContextUserReceiver)
                    {
                        foreach (var op in group.Items.OrderBy(n => n.Senders[0].Data.Sequence))
                        {
                            var decryptionResponse = await _holder.Connector.PayloadDecryptAsync(op.Payload);
                            if (decryptionResponse.Result != null)
                            {
                                if (decryptionResponse.Result.Result)
                                {
                                    payload += decryptionResponse.Result.UnencryptedPayload;
                                }
                                else
                                {
                                    payload += "Cannot decrypt, perhaps sender's public key has changed.";
                                }
                            }
                            else
                            {
                                InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                            }
                        }
                    }
                    else
                    {
                        payload = "Cannot read. Encrypted with sender's public key.";
                    }
                }

                if (passwordEncrypted)
                {
                    payload = "Password encrypted";
                }

                var message = new Message(group.Key.SenderAccount, isContextUserSender, group.Key.ReceiverAccount, isContextUserReceiver, group.Key.BlockNumber, payload, group.Key.PayloadType, group.Items.Count());
                messages.Add(message);
            }

            Messages = messages.OrderByDescending(n => n.BlockNumber).ToList();
            InfoMessage = null;

            _isBusy = false;
        }
    }
}
