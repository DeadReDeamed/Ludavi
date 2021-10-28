using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Server
{
    public class ServerClient
    {
        public User User { get; set; }
        public TCPHandler Handler { get; set; }
        public TcpClient Client { get; set; }
        public bool Connected { get; set; }
        public ServerClient(User user, TcpClient client, TCPHandler handler)
        {
            this.User = user;
            this.Handler = handler;
            this.Client = client;
            Connected = true;
            new Thread(async () => { startListening(); }).Start();
        }

        public async void startListening()
        {
            while (Connected)
            {
                string[] data = Handler.ReadMessage();

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
    }
}
