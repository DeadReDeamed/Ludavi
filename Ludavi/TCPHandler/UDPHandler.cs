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
        private Socket udpRecieverSocket;
        private Socket udpSendingSocket;
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
            sendingClient = new UdpClient(sendingPort, AddressFamily.InterNetwork);
            sendEndPoint = new IPEndPoint(Addres, sendingPort);
            udpSendingSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Udp);
            udpSendingSocket.Bind(sendEndPoint);

            receivingClient = new UdpClient(receivingPort, AddressFamily.InterNetwork);
            receivingEndPoint = new IPEndPoint(Addres, receivingPort);
            udpRecieverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Udp);
            udpRecieverSocket.Bind(receivingEndPoint);
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
            byte[] lengthFullPacket = BitConverter.GetBytes(tempPacket.Length);
            byte[] fullPacket = new byte[4 + tempPacket.Length];
            lengthFullPacket.CopyTo(fullPacket, 0);
            length.CopyTo(fullPacket, lengthFullPacket.Length);
            uid.CopyTo(fullPacket, length.Length + lengthFullPacket.Length);
            roomIdLength.CopyTo(fullPacket, length.Length + uid.Length + lengthFullPacket.Length);
            roomid.CopyTo(fullPacket, length.Length + uid.Length + roomIdLength.Length + lengthFullPacket.Length);
            message.CopyTo(fullPacket, length.Length + uid.Length + roomIdLength.Length + roomid.Length + lengthFullPacket.Length);
            udpSendingSocket.Send(tempPacket, SocketFlags.Broadcast);
            
        }

        public Tuple<Guid, uint, byte[]> ReceiveUdpMessage()
        {
            byte[] message = new byte[4];
            udpRecieverSocket.Receive(message, 0, message.Length, SocketFlags.Broadcast);
            message = new byte[BitConverter.ToInt32(message)];
            udpRecieverSocket.Receive(message, 0, message.Length, SocketFlags.Broadcast);
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
