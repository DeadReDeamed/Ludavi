using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPHandlerNamespace;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Server
{
    public class ServerClient
    {
        public User User { get; set; }
        public uint currentRoomid { get; set; }
        public TCPHandler Handler { get; set; }
        public TcpClient Client { get; set; }
        public UDPHandler UdpHandler { get; set; }
        public bool Connected { get; set; }
        public bool IsInVoice;
        public ServerClient(User user, TcpClient client, TCPHandler handler)
        {
            this.User = user;
            this.Handler = handler;
            this.Client = client;
            Connected = true;
            new Thread(async () => { startListening(); }).Start();
            UdpHandler = new UDPHandler();
        }

        public async void startListening()
        {
            while (Connected)
            {
                string[] data = await Handler.ReadMessage();

                try
                {
                    ServerLogic.functions[(TCPHandler.MessageTypes)int.Parse(data[(int)TCPHandler.StringIndex.TYPE])].Invoke(data); 
                } 
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public void startVoiceChat(int receivePort)
        {
            UdpHandler.SetReceivePoint(IPAddress.Any, receivePort);
            IsInVoice = true;
            new Thread(async () => {
                while (IsInVoice)
                {
                    Tuple<Guid, uint, byte[]> message = await UdpHandler.ReceiveUdpMessage();
                    ServerLogic.SendVoiceToAllUsers(message.Item1, message.Item2, message.Item3);
                }
            }).Start();
        }
    }
}
