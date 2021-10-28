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
        private uint userID;
        public RoomManager(TCPHandler handler, uint userID, MainWindowViewModel mainWindow)
        {
            this.userID = userID;
            rooms = new List<Room>();
          
            handler.SendMessage(userID, "server", TCPHandler.MessageTypes.ROOM, "GETROOMS");
            string[] message = handler.ReadMessage();
            string fullMessage = "";
            for (int i = (int)TCPHandler.StringIndex.MESSAGE; i < message.Length; i++)
            {
                fullMessage += message[i] + " ";
            }
            fullMessage = fullMessage.Trim();
            rooms = JsonConvert.DeserializeObject<List<Room>>(fullMessage);

            message = handler.ReadMessage();
            fullMessage = "";
            for (int i = (int)TCPHandler.StringIndex.MESSAGE; i < message.Length; i++)
            {
                fullMessage += message[i] + " ";
            }
            fullMessage = fullMessage.Trim();
            List<List<Message>> messagesList = JsonConvert.DeserializeObject<List<List<Message>>>(fullMessage);

            roomsAndMessages = new Dictionary<Room, List<Message>>();
            for(int i = 0; i < rooms.Count; i++)
            {
                roomsAndMessages.Add(rooms[i], messagesList[i]);
            }
            currentRoom = rooms[0];
            mainWindow.Messages = new ObservableCollectionEx<Message>(roomsAndMessages[currentRoom]);

        }

        public void SelectRoom(uint roomNumber)
        {
            foreach(Room room in rooms)
            {
                if(room.RoomID == roomNumber)
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

        public List<Message> GetMessagesFromRoom(uint roomID)
        {
            List<Message> messagesFromRoom = new List<Message>();
            foreach (KeyValuePair<Room, List<Message>> key in roomsAndMessages)
            {
                if (key.Key.RoomID == roomID)
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
