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
        public List<Message> list { get; set; }
        public MessageList(List<Message> list)
        {
            this.list = list;
        }
    }

    public class VoiceList : ServerList
    {
        public List<User> list { get; set; }
        public VoiceList(List<User> list)
        {
            this.list = list;
        }
    }
}
