using System;
using System.Windows.Threading;

namespace PascalWalletExtensionDemo.ViewModels
{
    public static class ViewModelHelper
    {
        public static void SetErrorMessage(IErrorMessageHolder holder, string errorMessage)
        {
            var timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            timer.Start();
            void TimerTick(object sender, EventArgs e)
            {
                timer.Stop();
                timer.Tick -= TimerTick;
                if (holder.IsBusy && holder.InfoMessage == null)
                {
                    holder.InfoMessage = new InfoMessageViewModel(errorMessage, null);
                }
            }
        }
    }
}
