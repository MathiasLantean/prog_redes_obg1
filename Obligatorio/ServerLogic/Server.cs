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
        private List<Student> students;

        public Server()
        {
            this.students = new List<Student>();
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
            Admin adminLog = new Admin() { User = user };
            if (this.admins.Contains(adminLog))
            {
                Admin currentAdmin = this.admins.Find(x => x.Equals(adminLog));
                return currentAdmin.User.Password == pass;
            }
            return false;
        }

        public void addStudent(string studentUsername, string studentPass)
        {
            User studentUser = new User() { Username = studentUsername, Password = studentPass };
            this.students.Add(new Student() { User = studentUser });
        }
    }
}
