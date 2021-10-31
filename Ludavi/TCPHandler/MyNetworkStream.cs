using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPHandlerNameSpace
{
    public interface INetworkStream
    {
        public void Write(byte[] data);
        public void Flush();
        public Task<int> ReadAsync(byte[] buffer, int offset, int size);
        public int Read(byte[] buffer, int offset, int size);
        public int Read(byte[] buffer);
    }
    class MyNetworkStream : INetworkStream
    {
        private NetworkStream stream;
        public MyNetworkStream(NetworkStream network)
        {
            this.stream = network;
        }

        public void Write(byte[] data)
        {
            this.stream.Write(data);
        }

        public void Flush()
        {
            this.stream.Flush();
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int size)
        {
            return this.stream.ReadAsync(buffer, offset, size);
        }
        public int Read(byte[] buffer, int offset, int size)
        {
            return this.stream.Read(buffer, offset, size);
        }
        public int Read(byte[] buffer)
        {
            return this.stream.Read(buffer);
        }
    }
}
