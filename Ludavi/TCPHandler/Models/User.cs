using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPHandlerNameSpace.Models
{
    public class User
    {
        public User(string name, Guid id)
        {
            this.UserId = id;
            this.Name = name;
            this.UserIdShort = "#" + UserId.ToString().Substring(0,4);
        }

        public Guid UserId { get; private set; }
        public string UserIdShort { get; private set; }
        public string Name { get; set; }


    }
}
