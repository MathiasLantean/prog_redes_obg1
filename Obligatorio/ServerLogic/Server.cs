using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ServerLogic
{
    public class Server
    {
        public static void Connect()
        {
            var tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 6000);
            tcpListener.Start(100);
            while (true)
            {
                var tcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine("SE CONECTO.");
                var networkStream = tcpClient.GetStream();
            }
        }
    }
}
