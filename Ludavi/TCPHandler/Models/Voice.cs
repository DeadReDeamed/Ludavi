using System;
using System.Collections.Generic;
using System.Text;

namespace TCPHandlerNameSpace.Models
{
    public class Voice : IRoomContent
    {
        public Voice(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
