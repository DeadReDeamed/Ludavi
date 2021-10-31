using Ludavi_Client.Util;
using Ludavi_Client.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Ludavi_Client.Models
{
    public class RoomManager
    {
        public Room currentRoom { get; private set; }
        public List<Room> rooms { get; private set; }
        public Dictionary<Room, List<Message>> roomsAndMessages;
        private Guid userID;
        private TCPHandler handler;
        private MainWindowViewModel mainWindow;
        private int indexOfCurrentRoom;
        public RoomManager(TCPHandler handler, Guid userID, MainWindowViewModel mainWindow)
        {
            this.userID = userID;
            rooms = new List<Room>();
            this.handler = handler;
            this.mainWindow = mainWindow;
            UpdateRooms();

        }

        public void UpdateRoomList(string dataMessage)
        {

            string[] dataString = dataMessage.Split(" ", 2);
            if (dataString[0] == "ROOMS")
            {
                if(rooms.Count == 0)
                {
                    indexOfCurrentRoom = 0;
                } else
                {
                    indexOfCurrentRoom = rooms.IndexOf(currentRoom);

                }
                rooms.Clear();
                rooms = JsonConvert.DeserializeObject<List<Room>>(dataString[1]);
            } else if (dataString[0] == "MESSAGES") 
            {
                List<List<Message>> messagesList = JsonConvert.DeserializeObject<List<List<Message>>>(dataString[1]);

                roomsAndMessages = new Dictionary<Room, List<Message>>();
                int messageListInt = 0;
                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].Type == (int)RoomType.Text)
                    {
                        roomsAndMessages.Add(rooms[i], messagesList[messageListInt]);
                        messageListInt++;
                    }
                }

                if (currentRoom == null)
                {
                    currentRoom = rooms[0];
                }
                else
                {
                    currentRoom = rooms[indexOfCurrentRoom];
                }
                mainWindow.Messages.Clear();
                if (rooms[indexOfCurrentRoom].Type == (int)RoomType.Text)
                {
                    mainWindow.Messages = new ObservableCollectionEx<Message>(roomsAndMessages[currentRoom]);
                } else
                {
                    mainWindow.Messages = new ObservableCollectionEx<Message>();
                }
                mainWindow.RoomsCollection.Clear();
                mainWindow.RoomsCollection = new ObservableCollectionEx<Room>(rooms);
            }
        }

        public async void UpdateRooms()
        {
            handler.SendMessage(userID, "server", TCPHandler.MessageTypes.ROOM, "GETROOMS");
        }

        public async void GetVoiceUsers()
        {
            handler.SendMessage(userID, "server", TCPHandler.MessageTypes.ROOM, "GETUSERSFROMROOM " + currentRoom.RoomID);
           
        }

        public void SelectRoom(uint roomID)
        {
            foreach(Room room in rooms)
            {
                if(room.RoomID == roomID)
                {
                    currentRoom = room;
                    break;
                }
            }
        }

        public void AddMessageToRoom(uint roomID, Message message)
        {
            foreach(KeyValuePair<Room, List<Message>> key in roomsAndMessages)
            {
                if(key.Key.RoomID == roomID)
                {
                    key.Value.Add(message);
                    break;
                }
            }
        }

        public List<Message> GetMessagesFromRoom()
        {
            List<Message> messagesFromRoom = new List<Message>();
            foreach (KeyValuePair<Room, List<Message>> key in roomsAndMessages)
            {
                if (key.Key.RoomID == currentRoom.RoomID)
                {
                    messagesFromRoom = key.Value;
                }
            }
            return messagesFromRoom;
        }

        public void addRoom(Room room)
        {
            rooms.Add(room);
        }
    }
}
