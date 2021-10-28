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
        public async Task<Task> SendMessage(uint id, string roomID, MessageTypes type, string message)
        {
            string dataString = $"{id} {roomID} {(int)type} {message}";
            byte[] length = BitConverter.GetBytes(dataString.Length);
            byte[] stringBytes = Encoding.ASCII.GetBytes(dataString);
            byte[] dataBytes = new byte[length.Length + stringBytes.Length];
            Encoding.ASCII.GetBytes(dataString).CopyTo(dataBytes, 4);
            length.CopyTo(dataBytes, 0);
            stream.Write(dataBytes);
            stream.Flush();
            return Task.CompletedTask;
        }

        public async Task<Task> SendMessage(string[] data)
        {
            string dataString = "";
            for(int i = 0; i < data.Length; i++)
            {
                dataString += data[i] + " ";
            }
            dataString = dataString.Trim();
            byte[] length = BitConverter.GetBytes(dataString.Length);
            byte[] stringBytes = Encoding.ASCII.GetBytes(dataString);
            byte[] dataBytes = new byte[length.Length + stringBytes.Length];
            Encoding.ASCII.GetBytes(dataString).CopyTo(dataBytes, 4);
            length.CopyTo(dataBytes, 0);
            stream.Write(dataBytes);
            stream.Flush();
            return Task.CompletedTask;
        }
        public string[] ReadMessage()
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            int length = BitConverter.ToInt32(buffer);
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            string dataString = Encoding.ASCII.GetString(buffer);
            string[] dataStringArray = dataString.Split(" ", 4);
            string message = dataStringArray[(int)StringIndex.MESSAGE];
            dataStringArray = new string[] {dataStringArray[0], dataStringArray[1], dataStringArray[2], message };
            stream.Flush();
            return dataStringArray;
        }
       

        public enum MessageTypes
        {
            CHAT = 0,
            VOICE = 1,
            DATA = 2,
            STATUS = 3,
            LOGIN = 4,
            ROOM = 5
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
