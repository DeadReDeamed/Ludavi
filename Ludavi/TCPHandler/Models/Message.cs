using System;
using System.Collections.Generic;
using System.Text;

namespace TCPHandlerNameSpace.Models
{
    public class Message : IRoomContent
    {
        public Message(string senderName, DateTime time, string content)
        {
            SenderName = senderName;
            Time = time;
            Content = content;
        }

        public string SenderName { get; set; }
        public DateTime Time { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return Content;
        }
    }

}
