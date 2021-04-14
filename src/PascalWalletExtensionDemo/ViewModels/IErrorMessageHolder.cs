namespace PascalWalletExtensionDemo.ViewModels
{
    public interface IErrorMessageHolder
    {
        bool IsBusy { get; }
        InfoMessageViewModel InfoMessage { get; set; }
    }
}
