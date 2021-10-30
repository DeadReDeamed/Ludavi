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
        private static Dictionary<Guid, ServerClient> clients;
        private static TcpListener tcpListener;
        private static TCPHandler tcpHandler;
        private static List<Room> rooms;
        private delegate T sendToAll<T>(string[] data);
        public static Dictionary<TCPHandler.MessageTypes, Action<string[]>> functions;
        public static Dictionary<Room, ServerList> roomsAndMessages;
        public static Dictionary<Room, List<UdpClient>> voiceRoomsWithUdp;
        public static List<User> usersInServer;

        //public static void main(string[] args)
        //{
        //    runserver();
        //}

        private delegate void Tasks(string[] data);
        public static void RunServer()
        {
            roomsAndMessages = new Dictionary<Room, ServerList>();
            clients = new Dictionary<Guid, ServerClient>();
            voiceRoomsWithUdp = new Dictionary<Room, List<UdpClient>>();
            functions = new Dictionary<TCPHandler.MessageTypes, Action<string[]>>();
            functions.Add(TCPHandler.MessageTypes.CHAT, SendChatToAllUsers);
            functions.Add(TCPHandler.MessageTypes.ROOM, HandleRoomManagement);
            functions.Add(TCPHandler.MessageTypes.VOICE, HandleVoiceData);
            usersInServer = new List<User>();
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 80);
            tcpListener.Start();
            rooms = new List<Room>();
            Room general = new Room("General", "Just your general chatroom", (int)RoomType.Text,0);
            rooms.Add(general);
            roomsAndMessages.Add(general, new MessageList(new List<Message>()));
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);

        }

        public static async void connectClientsToServer(IAsyncResult ar)
        {
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpHandler = new TCPHandler(tcpClient.GetStream());
            //Console.WriteLine($"Added: {tcpClient.Client.RemoteEndPoint} to server");
            string[] dataString = await tcpHandler.ReadMessage();

            int type = int.Parse(dataString[(int)TCPHandler.StringIndex.TYPE]);
            bool result = type switch
            {
                (int)TCPHandler.MessageTypes.REGISTER => await HandleRegisterRequest(dataString, tcpClient),
                (int)TCPHandler.MessageTypes.LOGIN => await HandleLoginRequest(dataString),
                _ => throw new Exception("Login request was not recognized")
            };

            if (!result) connectClientsToServer(ar);

            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectClientsToServer), null);
        }


        public static async Task<bool> HandleRegisterRequest(string[] dataString, TcpClient tcpClient)
        {
            string[] userValues = dataString[((int)TCPHandler.StringIndex.MESSAGE)].Split(' ', 2);
            Guid id = Guid.Parse(userValues[0]);
            string name = userValues[1];
            if (!clients.ContainsKey(id))
            {
                ServerClient newClient = new ServerClient(new User(name, id), tcpClient, tcpHandler);
                clients.Add(id, newClient);
                Console.WriteLine(clients[id].User.Name + " has joined Ludavi!");
                await clients[id].Handler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.REGISTER, $"ok");
                usersInServer.Add(new User(name, id));
                return true;
            }
            else
            {
                Console.WriteLine(clients[id].User.Name + " has tried to register a taken login, what a dummy :)");
                await clients[id].Handler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.REGISTER, $"already registered!");
                return false;
            }
        }

        public static async Task<bool> HandleLoginRequest(string[] dataString)
        {
            string[] userValues = dataString[((int)TCPHandler.StringIndex.MESSAGE)].Split(' ', 2);
            Guid id = Guid.Parse(userValues[0]);
            string name = userValues[1];
            if (clients.ContainsKey(id) && !clients[id].Connected)
            {
                Console.WriteLine(clients[id].User.Name + " has logged in!");
                await clients[id].Handler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.LOGIN, $"ok");
                return true;
            }
            else {
                await tcpHandler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.LOGIN, $"login failed!");
                return false;
            }
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
            string[] messageValues = data[(int)TCPHandler.StringIndex.MESSAGE].Split(':', 2);
            ((MessageList)roomsAndMessages[currentRoom]).list.Add(new Message(messageValues[0] + "#" + data[(int)TCPHandler.StringIndex.ID].Substring(0, 4), DateTime.Now, messageValues[1])); ;
            SendMessageToAllUsers(data);
        }

        public static async void SendMessageToAllUsers(string[] data)
        {
            sendToAll<Task> send = tcpHandler.SendMessage;
            send -= tcpHandler.SendMessage;
            foreach(KeyValuePair<Guid, ServerClient> key in clients)
            {
                send += clients[key.Key].Handler.SendMessage;
            }
            await send.Invoke(data);
        } 
        
        public static async void HandleRoomManagement(string[] data)
        {
            //Console.WriteLine(data[(int)TCPHandler.StringIndex.MESSAGE]);
            string message = data[(int)TCPHandler.StringIndex.MESSAGE];
            var messageSplit = message.Split(" ", 2);
            switch (messageSplit[0])
            {
                case "GETROOMS":
                    await clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, "RETURNROOMS " + "ROOMS " + JsonConvert.SerializeObject(rooms));
                    List<List<Message>> messagesPerList = new List<List<Message>>();
                    List<List<User>> usersPerList = new List<List<User>>();
                    foreach(KeyValuePair<Room, ServerList> key in roomsAndMessages){
                        if(key.Key.Type == (int)RoomType.Text)
                        {
                            messagesPerList.Add(((MessageList)key.Value).list);
                        } 
                    }
                    await clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, "RETURNROOMS " + "MESSAGES " + JsonConvert.SerializeObject(messagesPerList));
                   
                    break;
                case "ADDROOM":
                    Room newRoom = JsonConvert.DeserializeObject<Room>(messageSplit[1]);
                    if (newRoom != null)
                    {
                        rooms.Add(newRoom);
                        string[] stringdata = { "s", "s", ((int)TCPHandler.MessageTypes.ROOM).ToString(), "UPDATEROOMS" };
                        if (newRoom.Type == (int)RoomType.Text)
                        {
                            roomsAndMessages.Add(newRoom, new MessageList(new List<Message>()));
                        }
                        else if (newRoom.Type == (int)RoomType.Voice)
                        {
                            roomsAndMessages.Add(newRoom, new VoiceList(new List<User>()));
                            voiceRoomsWithUdp.Add(newRoom, new List<UdpClient>());
                        }
                        SendMessageToAllUsers(stringdata);
                    }
                    break;
                case "GETUSERSFROMROOM":
                    foreach(KeyValuePair<Room, ServerList> key in roomsAndMessages)
                    {
                        if(key.Key.RoomID == uint.Parse(messageSplit[1]))
                        {
                            await clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, "RETURNUSERS " + JsonConvert.SerializeObject(((VoiceList)key.Value).list));
                            break;

                        }
                    }
                    break;
            }
            
        }

        public static async void HandleVoiceData(string[] data)
        {
            string message = data[(int)TCPHandler.StringIndex.MESSAGE];
            Room currentRoom = null;
            foreach (Room r in rooms)
            {
                if (r.RoomID == uint.Parse(data[(int)TCPHandler.StringIndex.ROOMID]))
                {
                    currentRoom = r;
                    break;
                }
            }
            User currentUser = null;
            foreach (User u in usersInServer)
            {
                if (u.UserId == Guid.Parse(data[(int)TCPHandler.StringIndex.ID]))
                {
                    currentUser = u;
                    break;
                }
            }
            if (message == "JOIN" && currentRoom != null && currentUser != null)
            {
                
                ((VoiceList)roomsAndMessages[currentRoom]).list.Add(currentUser);
                await clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), data[(int)TCPHandler.StringIndex.ROOMID], TCPHandler.MessageTypes.VOICE, "OK");
            } else if(message == "LEAVE" && currentRoom != null && currentUser != null)
            {
                ((VoiceList)roomsAndMessages[currentRoom]).list.Remove(currentUser);
                await clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), data[(int)TCPHandler.StringIndex.ROOMID], TCPHandler.MessageTypes.VOICE, "OK");
            }
        }

        public void Dispose()
        {
            foreach(KeyValuePair<Guid, ServerClient> c in clients)
            {
                c.Value.Connected = false;
                c.Value.Client.GetStream().Close();
            }
        }
    }


}
