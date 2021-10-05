using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPHandler;

namespace Server
{
    class ServerLogic
    {
        private static Dictionary<uint, ServerClient> clients;
        private static TcpListener tcpListener;

        //public static void main(string[] args)
        //{
        //    runserver();
        //}
        private delegate void Tasks(string[] data);
        public static void RunServer()
        {
            clients = new Dictionary<uint, ServerClient>();
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 80);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);

        }

        public static async void connectClientsToServer(IAsyncResult ar)
        {
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            Console.WriteLine($"Added: {tcpClient.Client.RemoteEndPoint} to server");
            string[] dataString = await TCPHandler.TCPHandler.ReadMessage(tcpClient.GetStream());

            if (dataString[((int)TCPHandler.TCPHandler.StringIndex.ID)] == "0") {
                uint randomId = (uint)(new Random().Next(1, int.MaxValue));
                while (clients.ContainsKey(randomId))
                {
                    randomId = (uint)(new Random().Next(1, int.MaxValue));
                }

                string[] usernameAndPassword = dataString[((int)TCPHandler.TCPHandler.StringIndex.MESSAGE)].Split(" ");
                clients.Add(randomId, new ServerClient(randomId, usernameAndPassword[0], usernameAndPassword[1], tcpClient));
                Console.WriteLine(dataString[((int)TCPHandler.TCPHandler.StringIndex.MESSAGE)]);
                await TCPHandler.TCPHandler.SendMessage(randomId, "", TCPHandler.TCPHandler.MessageTypes.LOGIN, $"Logged in, your id is: {randomId}", clients[randomId].Stream);
            }
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);
        }

        public static async void SendMessageToID(string[] data)
        {
            var receiverClient = clients[uint.Parse(data[((int)TCPHandler.TCPHandler.StringIndex.RECEIVER)])];
            await TCPHandler.TCPHandler.SendMessage(uint.Parse(data[((int)TCPHandler.TCPHandler.StringIndex.ID)]), receiverClient.ID.ToString(), TCPHandler.TCPHandler.MessageTypes.CHAT, data[((int)TCPHandler.TCPHandler.StringIndex.MESSAGE)], receiverClient.Stream);
        }

        
    }
}
