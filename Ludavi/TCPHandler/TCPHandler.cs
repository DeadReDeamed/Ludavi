using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets

namespace TCPHandler
{
    public class TCPHandler
    {
        public async Task SendMessage(string username, string password, string receiver, MessageTypes type, string message, NetworkStream stream)
        {
            string dataString = $"{username} {password} {receiver} {type.ToString()} {message}";
            byte[] length = BitConverter.GetBytes(dataString.Length);
            byte[] dataBytes = length + Encoding.ASCII.GetBytes(dataString);
            stream.Write(dataBytes, 0, dataBytes.Length);
            stream.Flush();
            return 
        }

        public static string[] ReadMessage() 
        { 
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            int length = BitConverter.ToInt32(buffer);
            stream.Read(buffer, 0, length);
            string dataString = Encoding.ASCII.GetString(buffer);
            string[] dataStringArray = dataString.Split(" ");

            return dataStringArray;
        }

        public enum MessageTypes
        {
            CHAT,
            VOICE,
            DATA,
            STATUS
        }
    }
}
