using Pascal.Wallet.Connector;

namespace PascalWalletExtensionDemo.ViewModels
{
    public interface IConnectorHolder
    {
        PascalConnector Connector { get; set; }
    }
}
