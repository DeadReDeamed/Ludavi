using Ludavi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private delegate void sendToAllNoData(Guid id, string roomID, TCPHandler.MessageTypes type, string message);
        private delegate void sendVoiceToAll(Guid id, uint RoomID, byte[] message);
        public static Dictionary<TCPHandler.MessageTypes, Action<string[]>> functions;
        public static Dictionary<Room, ServerList> roomsAndMessages;
        public static List<User> usersInServer;
        public static Dictionary<Room, List<Guid>> roomsAndUsers;
        public static List<int> portsInUse;

        private delegate void Tasks(string[] data);

        public void RunServer()
        {
            roomsAndMessages = new Dictionary<Room, ServerList>();
            clients = InitClients();
            roomsAndUsers = new Dictionary<Room, List<Guid>>();
            portsInUse = new List<int>();
            functions = new Dictionary<TCPHandler.MessageTypes, Action<string[]>>();

            functions.Add(TCPHandler.MessageTypes.CHAT, SendChatToAllUsers);
            functions.Add(TCPHandler.MessageTypes.ROOM, HandleRoomManagement);
            functions.Add(TCPHandler.MessageTypes.VOICE, HandleVoiceData);
            functions.Add(TCPHandler.MessageTypes.LEAVE, HandleLeaveRequest);
            usersInServer = new List<User>();
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 80);
            portsInUse.Add(80);
            tcpListener.Start();
            rooms = new List<Room>();
            Room general = new Room("General", "Just your general chatroom", (int)RoomType.Text,0);
            rooms.Add(general);
            roomsAndMessages.Add(general, new MessageList(new List<Message>()));
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectClientsToServer), null);

        }

        private Dictionary<Guid, ServerClient> InitClients()
        {
            Dictionary<Guid, ServerClient> clients = new Dictionary<Guid, ServerClient>();

            FileIO.TryGetUsersFromSaveFile(ref clients);

            return clients;
        }

        private async static void HandleLeaveRequest(string[] data)
        {
            Guid id = Guid.Parse(data[(int)TCPHandler.StringIndex.ID]);
            foreach (KeyValuePair<Guid, ServerClient> c in clients)
            {
                if (c.Key.Equals(id))
                {
                    clients[id].Handler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.LEAVE, $"ok");
                    c.Value.Client.GetStream().Close();
                    c.Value.Connected = false;
                    usersInServer.Remove(c.Value.User);
                }
            }
        }

        public static async void ConnectClientsToServer(IAsyncResult ar)
        {
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpHandler = new TCPHandler(new MyNetworkStream(tcpClient.GetStream()));
            //Console.WriteLine($"Added: {tcpClient.Client.RemoteEndPoint} to server");
            string[] dataString = tcpHandler.ReadMessage();
            
            int type = int.Parse(dataString[(int)TCPHandler.StringIndex.TYPE]);
            bool result = type switch
            {
                (int)TCPHandler.MessageTypes.REGISTER => await HandleRegisterRequest(dataString, tcpClient),
                (int)TCPHandler.MessageTypes.LOGIN => await HandleLoginRequest(dataString, tcpHandler, tcpClient),
                _ => throw new Exception("Login request was not recognized")
            };

            if (!result) ConnectClientsToServer(ar);

            tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectClientsToServer), null);
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
                clients[id].Handler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.REGISTER, $"ok");
                usersInServer.Add(new User(name, id));
                return true;
            }
            else
            {
                Console.WriteLine(clients[id].User.Name + " has tried to register a taken login, what a dummy :)");
                tcpHandler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.REGISTER, $"already registered!");
                return false;
            }
        }

        public static async Task<bool> HandleLoginRequest(string[] dataString, TCPHandler handler, TcpClient client)
        {
            string[] userValues = dataString[((int)TCPHandler.StringIndex.MESSAGE)].Split(' ', 2);
            Guid id = Guid.Parse(userValues[0]);
            string name = userValues[1];
            if (clients.ContainsKey(id) && !clients[id].Connected)
            {
                Console.WriteLine(clients[id].User.Name + " has logged in!");
                clients[id] = new ServerClient(clients[id].User, client, handler);
                clients[id].Handler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.LOGIN, $"ok");
                usersInServer.Add(new User(name, id));
                return true;
            }
            else {
                tcpHandler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.LOGIN, $"login failed!");
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
            ((MessageList)roomsAndMessages[currentRoom]).List.Add(new Message(messageValues[0] + "#" + data[(int)TCPHandler.StringIndex.ID].Substring(0, 4), DateTime.Now, messageValues[1])); ;
            SendMessageToAllUsers(data);
        }

        public static async void SendMessageToAllUsers(string[] data)
        {
            sendToAll<Task> send = null;
            foreach(KeyValuePair<Guid, ServerClient> key in clients)
            {
                send += clients[key.Key].Handler.SendMessage;
            }
            await send.Invoke(data);
        }

        public static async void SendUpdateVoiceToAllUsers(Room room)
        {
            sendToAllNoData send = null;
            foreach (KeyValuePair<Guid, ServerClient> key in clients)
            {
                send += clients[key.Key].Handler.SendMessage;
            }
            send.Invoke(Guid.Empty, room.RoomID.ToString(), TCPHandler.MessageTypes.ROOM, "RETURNUSERS " + JsonConvert.SerializeObject(((VoiceList)roomsAndMessages[room]).List));
        }

        public static void SendVoiceToAllUsers(Guid guid, uint roomID, byte[] message)
        {
            sendVoiceToAll send = null;
            Room room = null;
            foreach(Room r in rooms)
            {
                if(r.RoomID == roomID)
                {
                    room = r;
                    break;
                }
            }
            foreach (KeyValuePair<Guid, ServerClient> key in clients)
            {
                if (roomsAndUsers[room].Contains(key.Key) && key.Key != guid)
                {
                    send += clients[key.Key].UdpHandler.SendUdpMessage;
                }
            }
            if (send != null)
            {
                send.Invoke(guid, roomID, message);
            }
        }

        public static async void HandleRoomManagement(string[] data)
        {
            string message = data[(int)TCPHandler.StringIndex.MESSAGE];
            var messageSplit = message.Split(" ", 2);
            switch (messageSplit[0])
            {
                case "GETROOMS":
                    clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, "RETURNROOMS " + "ROOMS " + JsonConvert.SerializeObject(rooms));
                    List<List<Message>> messagesPerList = new List<List<Message>>();
                    List<List<User>> usersPerList = new List<List<User>>();
                    foreach(KeyValuePair<Room, ServerList> key in roomsAndMessages){
                        if(key.Key.Type == (int)RoomType.Text)
                        {
                            messagesPerList.Add(((MessageList)key.Value).List);
                        } 
                    }
                    clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, "RETURNROOMS " + "MESSAGES " + JsonConvert.SerializeObject(messagesPerList));
                   
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
                            roomsAndUsers.Add(newRoom, new List<Guid>());
                        }
                        SendMessageToAllUsers(stringdata);
                    }
                    break;
                case "GETUSERSFROMROOM":
                    foreach(KeyValuePair<Room, ServerList> key in roomsAndMessages)
                    {
                        if(key.Key.RoomID == uint.Parse(messageSplit[1]))
                        {
                            clients[Guid.Parse(data[(int)TCPHandler.StringIndex.ID])].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.ROOM, "RETURNUSERS " + JsonConvert.SerializeObject(((VoiceList)key.Value).List));
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
                ((VoiceList)roomsAndMessages[currentRoom]).List.Add(currentUser);
                roomsAndUsers[currentRoom].Add(currentUser.UserId);
                
                SendUpdateVoiceToAllUsers(currentRoom);
                int sendPort = new Random().Next(0, 1000);
                while (portsInUse.Contains(sendPort)) sendPort = new Random().Next(0, 1000);
                int receivePort = new Random().Next(0, 1000);
                while (portsInUse.Contains(receivePort)) receivePort = new Random().Next(0, 1000);
                clients[currentUser.UserId].StartVoiceChat(receivePort, sendPort, ((IPEndPoint)clients[currentUser.UserId].Client.Client.RemoteEndPoint).Address);
                clients[currentUser.UserId].Handler.SendMessage(Guid.Parse(data[(int)TCPHandler.StringIndex.ID]), "", TCPHandler.MessageTypes.VOICE, "OK " 
                    + sendPort + " " + receivePort
                    );
            } else if(message == "LEAVE" && currentRoom != null && currentUser != null)
            {
                clients[currentUser.UserId].IsInVoice = false;
                clients[currentUser.UserId].UdpHandler.close();
                portsInUse.Remove(clients[currentUser.UserId].UdpHandler.SendingPort);
                portsInUse.Remove(clients[currentUser.UserId].UdpHandler.ReceivingPort);

                ((VoiceList)roomsAndMessages[currentRoom]).List.Remove(currentUser);
                roomsAndUsers[currentRoom].Remove(currentUser.UserId);
                SendUpdateVoiceToAllUsers(currentRoom);

            }
        }

        public void Dispose()
        {
            foreach(KeyValuePair<Guid, ServerClient> c in clients)
            {
                c.Value.Connected = false;
                c.Value.IsInVoice = false;
            }

            FileIO.WriteUsersToSaveFile(clients);
        }
    }


}
