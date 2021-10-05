using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerClient
    {
        public string IP { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public NetworkStream Stream { get; private set; }
        public TcpClient client { get; private set; }

        public ServerClient(string IP, string UserName, string Password, TcpClient client)
        {
            this.IP = IP;
            this.UserName = UserName;
            this.Password = Password;
            this.client = client;
            this.Stream = client.GetStream();
            
        }
    }
}
