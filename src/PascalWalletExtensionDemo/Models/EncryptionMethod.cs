using Pascal.Wallet.Connector.DTO;

namespace PascalWalletExtensionDemo.Models
{
    public class EncryptionMethod
    {
        public string Name { get; }
        public PayloadMethod Method { get; }

        public EncryptionMethod(string name, PayloadMethod method)
        {
            Name = name;
            Method = method;
        }
    }
}
