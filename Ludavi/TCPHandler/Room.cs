using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludavi_Client.Models
{
    public class Room
    {

        public string Name { get; set; }
        public string Topic { get; set; }
        public int Type { get; set; }
        public uint RoomID { get; private set; }

        public Room(string name, string topic, int type, uint roomID)
        {
            this.Name = name;
            this.Topic = topic;
            this.Type = type;
            this.RoomID = roomID;
        }

        public override string ToString()
        {
            return $"{Name} ({(RoomType)Type})";
        }

    }
}
