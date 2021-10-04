using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServerLogic
    {
        private static Dictionary<string, ServerClient> clients;
        private static TcpListener tcpListener; 

        public static void Main(string[] args)
        {
            RunServer();
        }

        public static void RunServer()
        {
            clients = new Dictionary<string, ServerClient>();
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 60000);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);

        }

        public static void connectClientsToServer(IAsyncResult ar)
        {
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            Console.WriteLine($"Added: {tcpClient.Client.RemoteEndPoint} to server");
            string[] dataString = readData(tcpClient.GetStream());
            clients.Add(dataString[0], new ServerClient(tcpClient.Client.RemoteEndPoint.ToString(), dataString[0], dataString[1], tcpClient));
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);
        }

        public static string[] readData(NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            int length = BitConverter.ToInt32(buffer);
            stream.Read(buffer, 0, length);
            string dataString = Encoding.ASCII.GetString(buffer);
            string[] dataStringArray = dataString.Split(" ");

            return dataStringArray;
        }
    }
}
