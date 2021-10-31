using System;
using System.Collections.Generic;
using System.Text;

namespace TCPHandlerNameSpace.Models
{
    public interface ServerList
    {

    }

    public class MessageList : ServerList
    {
        public List<Message> List { get; set; }
        public MessageList(List<Message> list)
        {
            this.List = list;
        }
    }

    public class VoiceList : ServerList
    {
        public List<User> List { get; set; }
        public VoiceList(List<User> list)
        {
            this.List = list;
        }
    }
}
