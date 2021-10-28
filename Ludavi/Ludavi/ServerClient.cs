using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPHandlerNameSpace;

namespace Server
{
    public class ServerClient
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public uint ID { get; set; }
        public TCPHandler handler { get; set; }
        public TcpClient client { get; set; }
        public bool connected { get; set; }
        public ServerClient(uint ID, string UserName, string Password, TcpClient client, TCPHandler handler)
        {
            this.UserName = UserName;
            this.Password = Password;
            this.handler = handler;
            this.ID = ID;
            this.client = client;
            connected = true;
            new Thread(async () => { startListening(); }).Start();
        }

        public async void startListening()
        {
            while (connected)
            {
                string[] data = await handler.ReadMessage();

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
