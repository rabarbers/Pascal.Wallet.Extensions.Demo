namespace PascalWalletExtensionDemo.Models
{
    public class AccountInfo
    {
        public AccountInfo(string name, uint accountNumber)
        {
            Name = name;
            AccountNumber = accountNumber;
        }
        
        public string Name { get; private set; }
        public uint AccountNumber { get; private set; }
    }
}
