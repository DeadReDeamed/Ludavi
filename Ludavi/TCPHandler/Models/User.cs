using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TCPHandlerNameSpace.Models
{
    [Serializable]
    public class User
    {
        public User(string name, Guid id)
        {
            this.UserId = id;
            this.Name = name;
            this.UserIdShort = "#" + UserId.ToString().Substring(0,4);
        }

        [JsonInclude]
        public Guid UserId { get; private set; }

        [JsonInclude]
        public string UserIdShort { get; private set; }

        [JsonInclude]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name + UserIdShort;
        }


    }
}
