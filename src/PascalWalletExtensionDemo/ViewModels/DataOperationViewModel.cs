using Pascal.Wallet.Connector.DTO;
using PascalWalletExtensionDemo.Models;
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
        private const decimal MinFee = 0.0001M;
        
        private readonly IConnectorHolder _holder;
        private InfoMessageViewModel _infoMessage;
        private Account _senderAccount;
        private Account _signerAccount;
        private uint _receiverAccount;
        private decimal _amount;
        private decimal _fee;
        private bool _alwaysAddFee;
        private string _message;
        private string _guid;
        private int _maxLength;
        private int _messageLength;
        private int _capacity;
        private int _partCount;
        private List<Account> _accounts;
        private List<Account> _accountsWithPasc;
        private IList<EncryptionMethod> _encryptionMethods;
        private string _password;
        private EncryptionMethod _selectedEncryptionMethod;
        private bool _initialized;

        public DataOperationViewModel(IConnectorHolder connectorHolder)
        {
            _holder = connectorHolder;
            EncryptionMethods = EncryptionMethod.GetSupportedMethods();
            SetDefaults();

            ClearCommand = new RelayCommand(SetDefaults);
            RefreshCommand = new RelayCommandAsync(InitializeAsync, parameter => CanRefresh());
            SendCommand = new RelayCommandAsync(SendDataOperationAsync, parameter => CanSend());
        }

        public ICommand SendCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand GenerateGuidCommand { get; private set; }

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
                SenderAccount = null;
                OnPropertyChanged(nameof(Accounts));
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
                if(_selectedEncryptionMethod == value)
                {
                    return;
                }
                switch (value.Method)
                {
                    case PayloadMethod.Sender:
                        MaxLength = GetMessageLength(SenderAccount?.EncodedPublicKey);
                        break;
                    case PayloadMethod.Dest:
                        var timer = new DispatcherTimer();
                        timer.Tick += TimerTick;
                        timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
                        timer.Start();
                        void TimerTick(object sender, EventArgs e)
                        {
                            timer.Stop();
                            timer.Tick -= TimerTick;
                            if (!_initialized)
                            {
                                InfoMessage = new InfoMessageViewModel("Retrieving receiver's public key...", null);
                            }
                        }

                        var error = new InfoMessageViewModel("Failed to retrieve receiver's public key!", () => InfoMessage = null, true);
                        _holder.Connector.GetAccountAsync(ReceiverAccount).ContinueWith(task =>
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
                if(_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public Account SenderAccount
        {
            get { return _senderAccount; }
            set
            {
                if(_senderAccount != value)
                {
                    if (SelectedEncryptionMethod?.Method == PayloadMethod.Sender)
                    {
                        MaxLength = GetMessageLength(value.EncodedPublicKey);
                    }
                    AccountsWithPasc = value != null ? Accounts.Where(n => n.EncodedPublicKey == value.EncodedPublicKey).ToList() : new List<Account>();
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
                if(_signerAccount != value)
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
                    var timer = new DispatcherTimer();
                    timer.Tick += TimerTick;
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
                    timer.Start();
                    void TimerTick(object sender, EventArgs e)
                    {
                        timer.Stop();
                        timer.Tick -= TimerTick;
                        if (!_initialized)
                        {
                            InfoMessage = new InfoMessageViewModel("Retrieving receiver's public key...", null);
                        }
                    }

                    _holder.Connector.GetAccountAsync(value).ContinueWith(task =>
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

        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if(_amount != value)
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
                if(_fee != value)
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

        public string Message
        {
            get { return _message; }
            set
            {
                if(_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                    MessageLength = value == null ? 0 : value.Length;
                }
            }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                if(_maxLength != value)
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
                if(_messageLength != value)
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
                if(_partCount != value)
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
            _initialized = false;

            var timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            timer.Start();
            void TimerTick(object sender, EventArgs e)
            {
                timer.Stop();
                timer.Tick -= TimerTick;
                if (!_initialized)
                {
                    InfoMessage = new InfoMessageViewModel("Loading accounts...", null);
                }
            }

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
            _initialized = true;
        }

        private async Task SendDataOperationAsync()
        {
            string errorMessage = null;
            for (var i = 0; i < PartCount; i++)
            {
                string messagePart = i < PartCount - 1 ? Message.Substring(i * MaxLength, MaxLength) : Message.Substring(i * MaxLength);

                var sendingDataResponse = await _holder.Connector.SendDataAsync(SenderAccount.AccountNumber, ReceiverAccount, _guid, SignerAccount?.AccountNumber,
                    DataType.ChatMessage, (uint)i, i == 0 ? Amount : 0, Fee / PartCount, messagePart, SelectedEncryptionMethod.Method, Password);
                if (errorMessage == null)
                {
                    errorMessage = sendingDataResponse.Error?.Message;
                }
            }
            if (errorMessage == null)
            {
                var manyMessagesText = PartCount > 1 ? $"consisting of {PartCount} parts " : "";
                InfoMessage = new InfoMessageViewModel($"Message {manyMessagesText}sent successfully.", () => { SetDefaults(); InfoMessage = null; });
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

        private bool CanRefresh()
        {
            return _initialized;
        }

        private bool CanSend()
        {
            return SenderAccount != null;
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
