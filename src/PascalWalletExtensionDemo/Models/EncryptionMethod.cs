using Pascal.Wallet.Connector.DTO;
using System.Collections.Generic;

namespace PascalWalletExtensionDemo.Models
{
    public class EncryptionMethod
    {
        public string Name { get; }
        public string Description { get; }
        public PayloadMethod Method { get; }

        public EncryptionMethod(string name, string description, PayloadMethod method)
        {
            Name = name;
            Description = description;
            Method = method;
        }

        public static IList<EncryptionMethod> GetSupportedMethods()
        {
            return new List<EncryptionMethod>
            {
                new EncryptionMethod("None", "Publicly visible", PayloadMethod.None),
                new EncryptionMethod("Receiver's public key", "Only the receiver can read", PayloadMethod.Dest),
                new EncryptionMethod("Sender's public key", "Only the sender can read", PayloadMethod.Sender),
                new EncryptionMethod("AES encryption", "Can read using the password", PayloadMethod.Aes)
            };
        }

        public static int GetMaxMessageLength(PayloadMethod payloadMethod, EncryptionType? encryptionType = null)
        {
            return payloadMethod switch
            {
                PayloadMethod.None => 255,
                PayloadMethod.Aes => 223,
                _ => 159
            };
        }
    }
}
