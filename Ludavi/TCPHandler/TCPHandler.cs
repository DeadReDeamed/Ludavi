using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets
using System.Threading.Tasks;

namespace TCPHandler
{
    public class TCPHandler
    {
        public async Task SendMessage(string username, string password, string receiver, MessageTypes type, string message, NetworkStream stream)
        {
            string dataString = $"{username} {password} {receiver} {type.ToString()} {message}";
            byte[] length = BitConverter.GetBytes(dataString.Length);
            byte[] dataBytes = length;
            Encoding.ASCII.GetBytes(dataString).CopyTo(dataBytes, 4);
            stream.Write(dataBytes, 0, dataBytes.Length);
            stream.Flush();
            
        }

        public static async Task<string[]> ReadMessage(NetworkStream stream) 
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
