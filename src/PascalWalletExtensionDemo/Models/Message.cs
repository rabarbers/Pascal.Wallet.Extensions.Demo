using Pascal.Wallet.Connector.DTO;
using System.Windows;
using System.Windows.Media;

namespace PascalWalletExtensionDemo.Models
{
    public class Message
    {
        public Message(uint sender, string senderName, bool senderIsContextUser, uint receiver, string receiverName, bool receiverIsContextUser, uint? blockNumber, int index, string payload, PayloadType payloadType, int parts)
        {
            SenderAccount = sender;
            SenderName = senderName;
            SenderIsContextUser = senderIsContextUser;
            BackgroundColor = new SolidColorBrush(senderIsContextUser ? Color.FromRgb(236,236,236) : Color.FromRgb(207, 255, 246));
            ReceiverAccount = receiver;
            ReceiverName = receiverName;
            ReceiverIsContextUser = receiverIsContextUser;
            BlockNumber = blockNumber;
            Index = index;
            Payload = payload;
            PayloadType = payloadType;
            Parts = parts;

        }

        public uint SenderAccount { get; set; }
        public string SenderName { get; set; }
        public uint ReceiverAccount { get; set; }
        public string ReceiverName { get; set; }
        public string Payload { get; set; }
        public uint? BlockNumber { get; set; }
        public int Index { get; set; }
        private PayloadType PayloadType { get; set; }
        private bool SenderIsContextUser { get; set; }
        private bool ReceiverIsContextUser { get; set; }

        //TODO quick dirty fix, need to cleanup
        public Brush BackgroundColor { get; set; }

        public int Parts { get; set; }

    }
}
