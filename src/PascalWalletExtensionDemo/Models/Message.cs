using Pascal.Wallet.Connector.DTO;
using System.Windows;

namespace PascalWalletExtensionDemo.Models
{
    public class Message
    {
        public Message(uint sender, bool senderIsContextUser, uint receiver, bool receiverIsContextUser, uint? blockNumber, string payload, PayloadType payloadType, int parts)
        {
            SenderAccount = sender;
            SenderIsContextUser = senderIsContextUser;
            SenderFontWeight = senderIsContextUser ? FontWeights.Normal : FontWeights.Bold;
            ReceiverFontWeight = receiverIsContextUser ? FontWeights.Normal : FontWeights.Bold;
            ReceiverAccount = receiver;
            ReceiverIsContextUser = receiverIsContextUser;
            BlockNumber = blockNumber;
            Payload = payload;
            PayloadType = payloadType;
            Parts = parts;
        }

        public uint SenderAccount { get; set; }
        public uint ReceiverAccount { get; set; }
        public string Payload { get; set; }
        public uint? BlockNumber { get; set; }
        private PayloadType PayloadType { get; set; }
        private bool SenderIsContextUser { get; set; }
        private bool ReceiverIsContextUser { get; set; }

        //TODO quick dirty fix, need to cleanup
        public FontWeight SenderFontWeight { get; set; }
        public FontWeight ReceiverFontWeight { get; set; }

        public int Parts { get; set; }

    }
}
