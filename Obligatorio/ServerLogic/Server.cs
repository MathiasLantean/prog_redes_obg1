using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Domain;
using Action = RouteController.Action;
using RouteController;

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
                var thread = new Thread(() => actionDespatcher(tcpClient));
                thread.Start();
            }
        }

        private void actionDespatcher(TcpClient tcpClient)
        {
            Console.WriteLine("SE CONECTO.");
            var networkStream = tcpClient.GetStream();

            while (true)
            {
                var actionInBytes = new byte[4];
                ReadDataFromStream(4, networkStream, actionInBytes);
                Action action = (Action)BitConverter.ToInt32(actionInBytes, 0);

                var contentTypeInBytes = new byte[4];
                ReadDataFromStream(4, networkStream, contentTypeInBytes);
                ContentType contentType = (ContentType)BitConverter.ToInt32(contentTypeInBytes, 0);
                
                var dataLengthInBytes = new byte[4];
                ReadDataFromStream(4, networkStream, dataLengthInBytes);
                var dataLength = BitConverter.ToInt32(dataLengthInBytes, 0);

                var dataBytes = new byte[dataLength];
                ReadDataFromStream(dataLength, networkStream, dataBytes);
                string data = Encoding.UTF8.GetString(dataBytes);

                ActionDispatcher actionDisp = new ActionDispatcher(action, contentType, data);
                actionDisp.dispatch(action);
            }
        }
        private static void ReadDataFromStream(int length, NetworkStream networkStream, byte[] dataBytes)
        {
            var totalRecivedData = 0;
            while (totalRecivedData < length)
            {
                var recived = networkStream.Read(dataBytes, totalRecivedData, length - totalRecivedData);
                if (recived == 0)
                {
                    //ERROR
                }
                totalRecivedData = recived;
            }
            totalRecivedData = 0;
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
