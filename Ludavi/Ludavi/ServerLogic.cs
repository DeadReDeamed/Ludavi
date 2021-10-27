using Ludavi_Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPHandlerNameSpace;

namespace Server
{
    public class ServerLogic
    {
        private static Dictionary<uint, ServerClient> clients;
        private static TcpListener tcpListener;
        private static TCPHandler tcpHandler;
        private static List<Room> rooms;
        private delegate T sendToAll<T>(string[] data);
        public static Dictionary<TCPHandler.MessageTypes, Action<string[]>> functions;

        //public static void main(string[] args)
        //{
        //    runserver();
        //}

        private delegate void Tasks(string[] data);
        public static void RunServer()
        {
            clients = new Dictionary<uint, ServerClient>();
            functions = new Dictionary<TCPHandler.MessageTypes, Action<string[]>>();
            functions.Add(TCPHandler.MessageTypes.CHAT, SendMessageToAllUsers);
            functions.Add(TCPHandler.MessageTypes.ROOM, HandleRoomManagement);
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 80);
            tcpListener.Start();
            rooms = new List<Room>();
            rooms.Add(new Room("testRoom", "bla bla", (int)RoomType.Text, 10));
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);

        }

        public static async void connectClientsToServer(IAsyncResult ar)
        {
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpHandler = new TCPHandler(tcpClient.GetStream());
            Console.WriteLine($"Added: {tcpClient.Client.RemoteEndPoint} to server");
            string[] dataString = tcpHandler.ReadMessage();

            if (dataString[((int)TCPHandler.StringIndex.ID)] == "0") {
                uint randomId = (uint)(new Random().Next(1, int.MaxValue));
                while (clients.ContainsKey(randomId))
                {
                    randomId = (uint)(new Random().Next(1, int.MaxValue));
                }

                string[] usernameAndPassword = dataString[((int)TCPHandler.StringIndex.MESSAGE)].Split(" ");
                clients.Add(randomId, new ServerClient(randomId, usernameAndPassword[0], usernameAndPassword[1], tcpClient, tcpHandler));
                Console.WriteLine(dataString[((int)TCPHandler.StringIndex.MESSAGE)]);
                await clients[randomId].handler.SendMessage(randomId, "", TCPHandler.MessageTypes.LOGIN, $"Logged in, your id is: {randomId}");
            }
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);
        }

        public static async void SendMessageToAllUsers(string[] data)
        {
            sendToAll<Task> send = tcpHandler.SendMessage;
            send -= tcpHandler.SendMessage;
            foreach(KeyValuePair<uint,ServerClient> key in clients)
            {
                send += clients[key.Key].handler.SendMessage;
            }
            await send.Invoke(data);
        } 
        
        public static async void HandleRoomManagement(string[] data)
        {
            Console.WriteLine(data[(int)TCPHandler.StringIndex.MESSAGE]);
            string message = data[(int)TCPHandler.StringIndex.MESSAGE];
            var messageSplit = message.Split(" ", 2);
            switch (messageSplit[0])
            {
                case "GETROOMS":
                    await clients[uint.Parse(data[(int)TCPHandler.StringIndex.ID])].handler.SendMessage(uint.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, JsonConvert.SerializeObject(rooms));
                    break;
                case "ADDROOM":
                    rooms.Add(JsonConvert.DeserializeObject<Room>(messageSplit[1]));
                    string[] stringdata = { "s", "s", ((int)TCPHandler.MessageTypes.ROOM).ToString(), "UPDATEROOMS" };
                    SendMessageToAllUsers(stringdata);
                    break;
            }
        }
    }
}
