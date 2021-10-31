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
        private UdpClient sendingClient;
        private Socket udpSocket;
        private UdpClient receivingClient;
        private IPEndPoint receivingEndPoint;
        private IPEndPoint sendingEndPoint;
        private bool FirstTimeConnect;
        private IPAddress receiverAddress;
        public int SendingPort { get; set; }
        public UDPHandler()
        {
            sendingClient = new UdpClient();
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            FirstTimeConnect = true;
        }

        public void Connect(string ip, int port)
        {
            sendingEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            sendingClient.Connect(sendingEndPoint);
            FirstTimeConnect = false;
        }

        public void SetReceivePoint(IPAddress ip, int receivingPort)
        {
            receivingEndPoint = new IPEndPoint(ip, receivingPort);
            receivingClient = new UdpClient(receivingEndPoint);
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
            sendingClient.Send(tempPacket, tempPacket.Length);
        }

        public async Task<Tuple<Guid, uint, byte[]>> ReceiveUdpMessage()
        {
            byte[] message;
            var result = await receivingClient.ReceiveAsync();
            message = result.Buffer;
            
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
            if (FirstTimeConnect)
            {
                receiverAddress = receivingEndPoint.Address;
                Connect(receiverAddress, sendingport);
            }

            return new Tuple<Guid ,uint, byte[]>(id, roomId, messageBytes);
        }
    }
}
