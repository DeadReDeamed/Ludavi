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
    class RoomManager
    {
        private uint currentRoom;
        private List<Room> rooms;
        private uint userID;
        public RoomManager(TCPHandler handler, uint userID)
        {
            this.userID = userID;
            rooms = new List<Room>();
           
            new Thread(async () => {
                await handler.SendMessage(userID, "server", TCPHandler.MessageTypes.DATA, "REQUEST ROOMS");
                string[] message = await handler.ReadMessage();
                string fullMessage = "";
                for (int i = 0; i < message.Length; i++)
                {
                    fullMessage += message[i] + " ";
                }
                fullMessage.Trim();
                rooms = JsonConvert.DeserializeObject<List<Room>>(fullMessage);
            });

        }
    }
}
