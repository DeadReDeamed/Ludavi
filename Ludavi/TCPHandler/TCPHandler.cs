using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCPHandlerNameSpace
{
    public class TCPHandler
    {
        private NetworkStream stream;

        public TCPHandler(NetworkStream stream)
        {
            this.stream = stream;
        }
        public async Task SendMessage(uint id, string roomID, MessageTypes type, string message)
        {
            string dataString = $"{id} {roomID} {type.ToString()} {message}";
            byte[] length = BitConverter.GetBytes(dataString.Length);
            byte[] stringBytes = Encoding.ASCII.GetBytes(dataString);
            byte[] dataBytes = new byte[length.Length + stringBytes.Length];
            Encoding.ASCII.GetBytes(dataString).CopyTo(dataBytes, 4);
            length.CopyTo(dataBytes, 0);
            stream.Write(dataBytes);
            stream.Flush();
        }

        public async Task<string[]> ReadMessage()
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            int length = BitConverter.ToInt32(buffer);
            buffer = new byte[length];
            //int currentLength = 0;
            //if(length > 1496)
            //{
            //    while(length > 1500)
            //    {
            //        stream.Read(buffer, currentLength, 1500);

            //    }
            //}
            stream.Read(buffer, 0, length);
            string dataString = Encoding.ASCII.GetString(buffer);
            string[] dataStringArray = dataString.Split(" ");
            string message = "";
            for(int i = ((int)StringIndex.MESSAGE); i < dataStringArray.Length; i++)
            {
                message += dataStringArray[i] + " ";
            }

            dataStringArray = new string[] {dataStringArray[0], dataStringArray[1], dataStringArray[2], message };
            return dataStringArray;
        }
       

        public enum MessageTypes
        {
            CHAT,
            VOICE,
            DATA,
            STATUS,
            LOGIN
        }

        public enum StringIndex
        {
            ID = 0,
            ROOMID = 1,
            TYPE = 2,
            MESSAGE = 3
        }
    }
}
