using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Domain;

namespace ServerLogic
{
    public class Server
    {
        private List<Admin> admins;

        public Server()
        {
            this.admins = new List<Admin>();
            this.admins.Add(new Admin());
        }

        public void Start()
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

        public bool LoginAdmin(string username, string pass)
        {
            User user = new User() { Username = username, Password = pass };
            return admins.Contains(new Admin() { User = user});
        }
    }
}
