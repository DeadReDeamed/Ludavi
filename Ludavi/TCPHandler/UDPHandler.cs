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
        private Socket udpSocket;
        private UdpClient receivingClient;
        private UdpClient sendingClient;
        private IPEndPoint receivingEndPoint;
        private IPEndPoint sendEndPoint;
        public int ReceivingPort { get; set; }
        public IPEndPoint SendingEndPoint { get; set; }
        public IPAddress ReceiverAddress { get; set; }
        public int SendingPort { get; set; }
        public UDPHandler()
        {

        }

        public void Connect(int sendingPort, int receivingPort, IPAddress Addres)
        {
            sendingClient = new UdpClient(sendingPort, AddressFamily.InterNetworkV6);
            sendEndPoint = new IPEndPoint(Addres, sendingPort);

            receivingClient = new UdpClient(receivingPort, AddressFamily.InterNetworkV6);
            receivingEndPoint = new IPEndPoint(Addres, receivingPort);
            this.ReceivingPort = receivingPort;
            this.SendingPort = sendingPort;
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
            sendingClient.Send(tempPacket, tempPacket.Length, sendEndPoint);
        }

        public Tuple<Guid, uint, byte[]> ReceiveUdpMessage()
        {
            byte[] message;
            message = receivingClient.Receive(ref receivingEndPoint);
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
            
            return new Tuple<Guid ,uint, byte[]>(id, roomId, messageBytes);
        }

        public void close()
        {
            sendingClient.Close();
            receivingClient.Close();
        }
    }
}
