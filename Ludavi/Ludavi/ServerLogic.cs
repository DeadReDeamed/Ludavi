using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Server
{
    public class ServerLogic : IDisposable
    {
        private static Dictionary<uint, ServerClient> clients;
        private static TcpListener tcpListener;
        private static TCPHandler tcpHandler;
        private static List<Room> rooms;
        private delegate T sendToAll<T>(string[] data);
        public static Dictionary<TCPHandler.MessageTypes, Action<string[]>> functions;
        public static Dictionary<Room, List<Message>> roomsAndMessages;

        //public static void main(string[] args)
        //{
        //    runserver();
        //}

        private delegate void Tasks(string[] data);
        public static void RunServer()
        {
            roomsAndMessages = new Dictionary<Room, List<Message>>();
            clients = new Dictionary<uint, ServerClient>();
            functions = new Dictionary<TCPHandler.MessageTypes, Action<string[]>>();
            functions.Add(TCPHandler.MessageTypes.CHAT, SendChatToAllUsers);
            functions.Add(TCPHandler.MessageTypes.ROOM, HandleRoomManagement);
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 80);
            tcpListener.Start();
            rooms = new List<Room>();
            Room general = new Room("General", "Just your general chatroom", (int)RoomType.Text,0);
            rooms.Add(general);
            roomsAndMessages.Add(general, new List<Message>());
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);

        }

        public static async void connectClientsToServer(IAsyncResult ar)
        {
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpHandler = new TCPHandler(tcpClient.GetStream());
            Console.WriteLine($"Added: {tcpClient.Client.RemoteEndPoint} to server");
            string[] dataString = await tcpHandler.ReadMessage();

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

        public static async void SendChatToAllUsers(string[] data)
        {
            Room currentRoom = null;
            foreach(Room r in rooms)
            {
                if(r.RoomID == uint.Parse(data[(int)TCPHandler.StringIndex.ROOMID])){
                    currentRoom = r;
                }
            }
            roomsAndMessages[currentRoom].Add(new Message(data[(int)TCPHandler.StringIndex.ID], DateTime.Now, data[(int)TCPHandler.StringIndex.MESSAGE]));
            SendMessageToAllUsers(data);
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
                    List<List<Message>> messagesPerList = new List<List<Message>>();
                    foreach(KeyValuePair<Room,List<Message>> key in roomsAndMessages){
                        messagesPerList.Add(key.Value);
                    }
                    await clients[uint.Parse(data[(int)TCPHandler.StringIndex.ID])].handler.SendMessage(uint.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, JsonConvert.SerializeObject(messagesPerList));

                    break;
                case "ADDROOM":
                    string[] stringdata = { "s", "s", ((int)TCPHandler.MessageTypes.ROOM).ToString(), "UPDATEROOMS" };
                    Room newRoom = JsonConvert.DeserializeObject<Room>(messageSplit[1]);
                    rooms.Add(newRoom);
                    roomsAndMessages.Add(newRoom, new List<Message>());
                    SendMessageToAllUsers(stringdata);
                    break;
            }
        }

        public void Dispose()
        {
            foreach(KeyValuePair<uint, ServerClient> c in clients)
            {
                c.Value.connected = false;
                c.Value.client.GetStream().Close();
            }
        }
    }


}
