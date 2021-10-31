using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TCPHandlerNamespace;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Server
{
    [Serializable]
    public class ServerClient
    {
        [JsonInclude]
        public User User { get; set; }

        [JsonInclude]
        public uint currentRoomid { get; set; }

        [JsonIgnore]
        public TCPHandler Handler { get; set; }

        [JsonIgnore]
        public TcpClient Client { get; set; }

        [JsonIgnore]
        public UDPHandler UdpHandler { get; set; }

        [JsonInclude]
        public bool Connected { get; set; }

        [JsonInclude]
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

        public ServerClient(User user)
        {
            this.User = user;
            this.Handler = null;
            this.Client = null;
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
                    Tuple<Guid, uint, byte[]> message = UdpHandler.ReceiveUdpMessage();
                    ServerLogic.SendVoiceToAllUsers(message.Item1, message.Item2, message.Item3);
                }
            }).Start();
        }
    }
}
