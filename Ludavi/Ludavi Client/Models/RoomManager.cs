using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPHandlerNameSpace;

namespace Ludavi_Client.Models
{
    public class RoomManager
    {
        public Room currentRoom { get; private set; }
        public List<Room> rooms { get; private set; }
        private uint userID;
        public RoomManager(TCPHandler handler, uint userID)
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

        public void addRoom(Room room)
        {
            rooms.Add(room);
        }
    }
}
