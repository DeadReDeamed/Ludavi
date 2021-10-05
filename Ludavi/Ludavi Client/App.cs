using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TCPHandler;

namespace Ludavi_Client
{
    partial class App : Application
    {
        public uint ID { get; private set; }
        public async void connectToServer()
        {
            AllocConsole();
            TcpClient client = new TcpClient();
            await client.ConnectAsync("localhost", 80);
            await TCPHandler.TCPHandler.SendMessage(0, "", TCPHandler.TCPHandler.MessageTypes.LOGIN, "Luca Password", client.GetStream());
            string[] message = await TCPHandler.TCPHandler.ReadMessage(client.GetStream());
            Console.WriteLine(message[((int)TCPHandler.TCPHandler.StringIndex.MESSAGE)]);
            ID = uint.Parse(message[((int)TCPHandler.TCPHandler.StringIndex.ID)]);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
