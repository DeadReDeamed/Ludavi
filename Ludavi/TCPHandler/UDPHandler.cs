using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPHandlerNamespace
{
    public class UDPHandler
    {
        private UdpClient client;
        private Socket udpSocket;
        private UdpClient receivingClient;
        private IPEndPoint receivingEndPoint;
        private bool firstTimeConnect;

        public IPEndPoint SendingEndPoint { get; set; }
        public IPAddress ReceiverAddress { get; set; }
        public int SendingPort { get; set; }
        public UDPHandler()
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            firstTimeConnect = true;
        }

        public void Connect(int port)
        {
            client = new UdpClient(port);
            SendingPort = port;
        }

        public void SetReceivePoint(IPAddress ip, int receivingPort)
        {
            receivingEndPoint = new IPEndPoint(ip, receivingPort);
        }
        public async void SendUdpMessage(Guid id, uint RoomID, byte[] message)
        {
            byte[] uid = Encoding.ASCII.GetBytes(id.ToString());
            byte[] roomid = BitConverter.GetBytes(RoomID);
            byte[] length = BitConverter.GetBytes(uid.Length);
            byte[] roomIdLength = BitConverter.GetBytes(roomid.Length);
            byte[] tempPacket = new byte[length.Length + uid.Length + roomIdLength.Length + roomid.Length + message.Length];
            length.CopyTo(tempPacket, 0);
            uid.CopyTo(tempPacket, length.Length);
            roomIdLength.CopyTo(tempPacket, length.Length + uid.Length);
            roomid.CopyTo(tempPacket, length.Length + uid.Length + roomIdLength.Length);
            message.CopyTo(tempPacket, length.Length + uid.Length + roomIdLength.Length + roomid.Length);
            client.Send(tempPacket, tempPacket.Length, ReceiverAddress.ToString(), SendingPort);
        }

        public Tuple<Guid, uint, byte[]> ReceiveUdpMessage()
        {
            byte[] message;
            message = client.Receive(ref receivingEndPoint);
            
            int lengthGuid = BitConverter.ToInt32(message, 0);
            byte[] guidBytes = new byte[lengthGuid];
            int startIndex = 4;
            Array.Copy(message, startIndex, guidBytes, 0, guidBytes.Length);
            Guid id = Guid.Parse(Encoding.ASCII.GetString(guidBytes));

            int lengthRoomId = BitConverter.ToInt32(message, lengthGuid + startIndex);
            byte[] roomIdBytes = new byte[lengthRoomId];
            startIndex += lengthGuid + 4;
            Array.Copy(message, startIndex, roomIdBytes, 0, roomIdBytes.Length);
            uint roomId = BitConverter.ToUInt32(roomIdBytes);

            startIndex += roomIdBytes.Length;
            byte[] messageBytes = new byte[message.Length - startIndex - roomIdBytes.Length];
            Array.Copy(message, startIndex, messageBytes, 0, messageBytes.Length);
            if (firstTimeConnect)
            {
                ReceiverAddress = receivingEndPoint.Address;
                firstTimeConnect = false;
            }

            return new Tuple<Guid ,uint, byte[]>(id, roomId, messageBytes);
        }
    }
}
