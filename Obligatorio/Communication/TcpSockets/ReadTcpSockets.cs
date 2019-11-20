using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.TcpSockets
{
    public class ReadTcpSockets
    {
        private readonly TcpClient _tcpClient;

        public ReadTcpSockets(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public async Task<byte[]> ReadDataAsync(int dataLength)
        {
            var totalDataReceived = 0;
            var data = new byte[dataLength];
            var networkStream = _tcpClient.GetStream();
            while (totalDataReceived < dataLength)
            {
                var dataReceived =
                    await networkStream.ReadAsync(data, totalDataReceived, dataLength - totalDataReceived);
                if (dataReceived == 0)
                {
                    throw new SocketException();
                }
                totalDataReceived += dataReceived;
            }

            return data;
        }
    }
}

