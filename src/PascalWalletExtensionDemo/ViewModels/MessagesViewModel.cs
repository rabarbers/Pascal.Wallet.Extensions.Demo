﻿using Pascal.Wallet.Connector;
using Pascal.Wallet.Connector.DTO;
using PascalWalletExtensionDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PascalWalletExtensionDemo.ViewModels
{
    public class MessagesViewModel: ViewModelBase, IErrorMessageHolder
    {
        private const decimal MinFee = 0.0001M;

        private readonly IConnectorHolder _connectorHolder;
        private readonly IPasswordsHolder _passwordsHolder;
        private InfoMessageViewModel _infoMessage;
        private bool _isBusy;
        private List<Account> _accounts;
        private List<Account> _signerAccounts;
        private List<Message> _messages;
        private IList<EncryptionMethod> _encryptionMethods;
        private EncryptionMethod _selectedEncryptionMethod;
        private string _password;
        private Account _senderAccount;
        private Account _signerAccount;
        private uint _receiverAccount;
        private string _message;
        private decimal _amount;
        private decimal _fee;
        private bool _alwaysAddFee;
        private int _maxLength;
        private int _messageLength;
        private int _capacity;
        private int _partCount;
        private string _guid;

        public MessagesViewModel(IConnectorHolder connectorHolder, IPasswordsHolder passwordsHolder)
        {
            _connectorHolder = connectorHolder;
            _passwordsHolder = passwordsHolder;

            EncryptionMethods = EncryptionMethod.GetSupportedMethods();
            SetDefaults();

            RefreshCommand = new RelayCommandAsync(InitializeAsync);
            SendCommand = new RelayCommandAsync(SendDataOperationAsync, parameter => CanSend());
        }

        public ICommand SendCommand { get; private set; }
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

        public IList<EncryptionMethod> EncryptionMethods
        {
            get { return _encryptionMethods; }
            set
            {
                _encryptionMethods = value;
                OnPropertyChanged(nameof(EncryptionMethods));
            }
        }

        public EncryptionMethod SelectedEncryptionMethod
        {
            get { return _selectedEncryptionMethod; }
            set
            {
                if (_selectedEncryptionMethod == value)
                {
                    return;
                }
                switch (value.Method)
                {
                    case PayloadMethod.Sender:
                        MaxLength = GetMessageLength(SenderAccount?.EncodedPublicKey);
                        break;
                    case PayloadMethod.Dest:
                        ViewModelHelper.SetErrorMessage(this, "Retrieving receiver's public key...");

                        var error = new InfoMessageViewModel("Failed to retrieve receiver's public key!", () => InfoMessage = null, true);
                        _connectorHolder.Connector.GetAccountAsync(ReceiverAccount).ContinueWith(task =>
                        {
                            MaxLength = GetMessageLength(task.Result?.Result?.EncodedPublicKey);
                            InfoMessage = task.Result.Result == null ? error : null;
                        });
                        break;
                    default:
                        MaxLength = EncryptionMethod.GetMaxMessageLength(value.Method);
                        break;
                }
                _selectedEncryptionMethod = value;
                OnPropertyChanged(nameof(SelectedEncryptionMethod));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
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

        public List<Account> SignerAccounts
        {
            get { return _signerAccounts; }
            set
            {
                _signerAccounts = value;
                OnPropertyChanged(nameof(SignerAccounts));
            }
        }


        public Account SenderAccount
        {
            get { return _senderAccount; }
            set
            {
                if (_senderAccount != value)
                {
                    if (SelectedEncryptionMethod?.Method == PayloadMethod.Sender)
                    {
                        MaxLength = GetMessageLength(value.EncodedPublicKey);
                    }
                    SignerAccounts = value != null ? Accounts.Where(n => n.EncodedPublicKey == value.EncodedPublicKey).ToList() : new List<Account>();
                    _senderAccount = value;
                    OnPropertyChanged(nameof(SenderAccount));
                }
            }
        }

        public Account SignerAccount
        {
            get { return _signerAccount; }
            set
            {
                if (_signerAccount != value)
                {
                    _signerAccount = value;
                    OnPropertyChanged(nameof(SignerAccount));
                }
            }
        }

        public uint ReceiverAccount
        {
            get { return _receiverAccount; }
            set
            {
                if (_receiverAccount == value)
                {
                    return;
                }
                if (SelectedEncryptionMethod?.Method == PayloadMethod.Dest)
                {
                    ViewModelHelper.SetErrorMessage(this, "Retrieving receiver's public key...");

                    _connectorHolder.Connector.GetAccountAsync(value).ContinueWith(task =>
                    {
                        if (task.Result.Result != null)
                        {
                            MaxLength = GetMessageLength(task.Result.Result.EncodedPublicKey);
                            InfoMessage = null;
                        }
                        else
                        {
                            InfoMessage = new InfoMessageViewModel("Failed to retrieve receiver's public key!", () => InfoMessage = null, true);
                        }
                    });
                }

                _receiverAccount = value;
                OnPropertyChanged(nameof(ReceiverAccount));
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

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                    MessageLength = value == null ? 0 : value.Length;
                }
            }
        }

        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        public decimal Fee
        {
            get { return _fee; }
            set
            {
                if (_fee != value)
                {
                    _fee = value;
                    OnPropertyChanged(nameof(Fee));
                }
            }
        }

        public bool AlwaysAddFee
        {
            get { return _alwaysAddFee; }
            set
            {
                if (_alwaysAddFee != value)
                {
                    _alwaysAddFee = value;
                    OnPropertyChanged(nameof(AlwaysAddFee));
                    if (PartCount == 1)
                    {
                        Fee = value ? MinFee : 0;
                    }
                }
            }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                if (_maxLength != value)
                {
                    _maxLength = value;
                    OnPropertyChanged(nameof(MaxLength));
                    PartCount = MessageLength > 0 ? (int)Math.Ceiling((double)MessageLength / MaxLength) : 1;
                }
            }
        }

        public int MessageLength
        {
            get { return _messageLength; }
            set
            {
                if (_messageLength != value)
                {
                    _messageLength = value;
                    OnPropertyChanged(nameof(MessageLength));
                    PartCount = MessageLength > 0 ? (int)Math.Ceiling((double)MessageLength / MaxLength) : 1;
                    var remainder = value % MaxLength;
                    Capacity = value == 0 ? MaxLength : (remainder > 0 ? MaxLength - remainder : 0);
                }
            }
        }

        public int PartCount
        {
            get { return _partCount; }
            set
            {
                if (_partCount != value)
                {
                    _partCount = value;
                    OnPropertyChanged(nameof(PartCount));
                    Fee = value > 1 ? MinFee * value : (AlwaysAddFee ? MinFee : 0);
                }
            }
        }

        public int Capacity
        {
            get { return _capacity; }
            set
            {
                if (_capacity != value)
                {
                    _capacity = value;
                    OnPropertyChanged(nameof(Capacity));
                }
            }
        }

        public async Task InitializeAsync()
        {
            _isBusy = true;

            InfoMessage = new InfoMessageViewModel("Loading messages...", null);
            var accountsResponse = await _connectorHolder.Connector.GetWalletAccountsAsync(max: 500);
            if (accountsResponse.Result != null)
            {
                Accounts = accountsResponse.Result.OrderBy(n => n.AccountNumber).ToList();
            }
            else
            {
                InfoMessage = new InfoMessageViewModel("Failed to load accounts! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                return;
            }

            var operations = new List<Operation>();
            foreach (var account in Accounts)
            {
                var receivedMessagesResponse = await _connectorHolder.Connector.FindDataOperationsAsync(receiverAccount: account.AccountNumber, max: int.MaxValue);
                if (receivedMessagesResponse.Result != null)
                {
                    operations.AddRange(receivedMessagesResponse.Result);
                }
                else
                {
                    InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                    return;
                }

                var sentMessagesResponse = await _connectorHolder.Connector.FindDataOperationsAsync(senderAccount: account.AccountNumber, max: int.MaxValue);
                if (sentMessagesResponse.Result != null)
                {
                    foreach(var op in sentMessagesResponse.Result)
                    {
                        //TODO use dictionary
                        var isContextUserSender = Accounts.Any(n => n.AccountNumber == op.Senders[0].AccountNumber);
                        var isContextUserReceiver = Accounts.Any(n => n.AccountNumber == op.Receivers[0].AccountNumber);
                        //if sender and receiver is your account, then avoid duplicate messages
                        if (!isContextUserSender || !isContextUserReceiver)
                        {
                            operations.Add(op);
                        }
                    }
                }
                else
                {
                    InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                    return;
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
                            var decryptionResponse = await _connectorHolder.Connector.PayloadDecryptAsync(op.Payload);
                            if (decryptionResponse.Result != null)
                            {
                                if(decryptionResponse.Result.Result)
                                {
                                    payload += decryptionResponse.Result.UnencryptedPayload;
                                }
                                else
                                {
                                    payload = "Cannot decrypt, perhaps receiver's public key has changed.";
                                    break;
                                }
                            }
                            else
                            {
                                InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                                return;
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
                            var decryptionResponse = await _connectorHolder.Connector.PayloadDecryptAsync(op.Payload);
                            if (decryptionResponse.Result != null)
                            {
                                if (decryptionResponse.Result.Result)
                                {
                                    payload += decryptionResponse.Result.UnencryptedPayload;
                                }
                                else
                                {
                                    payload = "Cannot decrypt, perhaps sender's public key has changed.";
                                    break;
                                }
                            }
                            else
                            {
                                InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                                return;
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
                    foreach (var op in group.Items.OrderBy(n => n.Senders[0].Data.Sequence))
                    {
                        
                        var decryptionResponse = await _connectorHolder.Connector.PayloadDecryptAsync(op.Payload, _passwordsHolder.Passwords?.Split(Environment.NewLine) ?? new string[] { });
                        if (decryptionResponse.Result != null)
                        {
                            if (decryptionResponse.Result.Result)
                            {
                                payload += decryptionResponse.Result.UnencryptedPayload;
                            }
                            else
                            {
                                payload = "Cannot decrypt, wrong password.";
                                break;
                            }
                        }
                        else
                        {
                            InfoMessage = new InfoMessageViewModel("Failed to load messages! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                            return;
                        }
                    }
                }

                string senderName = string.Empty;
                if(Accounts.Any(n => n.AccountNumber == group.Key.SenderAccount))
                {
                    senderName = Accounts.Where(n => n.AccountNumber == group.Key.SenderAccount).FirstOrDefault()?.Name;
                }
                else
                {
                    var senderAccountResponse = await _connectorHolder.Connector.GetAccountAsync(group.Key.SenderAccount);
                    if (senderAccountResponse.Result != null)
                    {
                        senderName = senderAccountResponse.Result.Name;
                    }
                    else
                    {
                        InfoMessage = new InfoMessageViewModel("Failed to load sender name! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                        return;
                    }
                }

                string receiverName = string.Empty;
                if (Accounts.Any(n => n.AccountNumber == group.Key.SenderAccount))
                {
                    receiverName = Accounts.Where(n => n.AccountNumber == group.Key.ReceiverAccount).FirstOrDefault()?.Name;
                }
                else
                {
                    var receiverAccountResponse = await _connectorHolder.Connector.GetAccountAsync(group.Key.ReceiverAccount);
                    if (receiverAccountResponse.Result != null)
                    {
                        receiverName = receiverAccountResponse.Result.Name;
                    }
                    else
                    {
                        InfoMessage = new InfoMessageViewModel("Failed to load receiver name! Check if Pascal Wallet is open and if it accepts connections.", () => InfoMessage = null, true);
                        return;
                    }
                }


                var message = new Message(group.Key.SenderAccount, senderName, isContextUserSender, group.Key.ReceiverAccount, receiverName, isContextUserReceiver, group.Key.BlockNumber, payload, group.Key.PayloadType, group.Items.Count());
                messages.Add(message);
            }

            Messages = messages.OrderByDescending(n => n.BlockNumber).ToList();
            InfoMessage = null;

            _isBusy = false;
        }

        private async Task SendDataOperationAsync()
        {
            string errorMessage = null;
            for (var i = 0; i < PartCount; i++)
            {
                string messagePart = i < PartCount - 1 ? Message.Substring(i * MaxLength, MaxLength) : Message.Substring(i * MaxLength);

                var sendingDataResponse = await _connectorHolder.Connector.SendDataAsync(SenderAccount.AccountNumber, ReceiverAccount, _guid, SignerAccount?.AccountNumber,
                    DataType.ChatMessage, (uint)i, i == 0 ? Amount : 0, Fee / PartCount, messagePart, SelectedEncryptionMethod.Method, Password);
                if (errorMessage == null)
                {
                    errorMessage = sendingDataResponse.Error?.Message;
                }
            }
            if (errorMessage == null)
            {
                await InitializeAsync();
            }
            else
            {
                InfoMessage = new InfoMessageViewModel(errorMessage, () => InfoMessage = null, true);
            }
        }

        private void SetDefaults()
        {
            _guid = Guid.NewGuid().ToString().ToUpper();
            SelectedEncryptionMethod = EncryptionMethods[0];
            ReceiverAccount = Properties.Settings.Default.DefaultReceiver;
            SignerAccount = null;
            SenderAccount = null;
            Message = null;
            Fee = 0;
            Amount = 0;
            MaxLength = 255;
            Capacity = 255;
            MessageLength = 0;
            PartCount = 1;
            Password = null;
        }

        private bool CanSend()
        {
            return SenderAccount != null && (!string.IsNullOrEmpty(Message) || Amount > 0);
        }

        private static int GetMessageLength(string encodedPublicKey)
        {
            var encodedKeySize = encodedPublicKey?.Length ?? 0;
            return encodedKeySize switch
            {
                140 => 191,
                156 => 191,
                204 => 175,
                276 => 159,
                _ => 159, //if public key size not known, then assume the worst case scenario
            };
        }
    }
}
