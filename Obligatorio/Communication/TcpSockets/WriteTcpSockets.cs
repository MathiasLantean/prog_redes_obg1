using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.TcpSockets
{
    public class WriteTcpSockets
    {
        private readonly TcpClient _tcpClient;

        public WriteTcpSockets(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public async Task WriteDataAsync(byte[] data)
        {
            try
            {
                var networkStream = _tcpClient.GetStream();
                await networkStream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception)
            {
                throw new SocketException();
            }

        }
    }
}
