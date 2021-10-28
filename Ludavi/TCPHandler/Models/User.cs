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
        }

        public Guid UserId { get; set; }
        public string Name { get; set; }


    }
}
