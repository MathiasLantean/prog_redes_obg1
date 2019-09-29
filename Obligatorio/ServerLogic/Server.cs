using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Action = RouteController.Action;
using RouteController;

namespace ServerLogic
{
    public class Server
    {

        private ActionDispatcher route = new ActionDispatcher();

        public void Start()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                IPAddress serverIpAddress = IPAddress.Parse(appSettings["ServerIpAddress"]);
                int serverPort = Int32.Parse(appSettings["ServerPort"]);

                var tcpListener = new TcpListener(serverIpAddress, serverPort);
                tcpListener.Start(100);
                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();
                    var thread = new Thread(() => actionDespatcher(tcpClient));
                    thread.Start();
                }
            }
            catch (ConfigurationErrorsException)
            {
                throw new Exception("Error leyendo app settings.");
            }
            catch (FormatException)
            {
                throw new Exception("Server IP o puerto invalido.");
            }
        }

        private void actionDespatcher(TcpClient tcpClient)
        {

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SE CONECTÓ UN USUARIO.");
            Console.ResetColor();

            var networkStream = tcpClient.GetStream();

            while (true)
            {
                var actionInBytes = new byte[4];
                ReadDataFromStream(4, networkStream, actionInBytes);
                Action action = (Action)BitConverter.ToInt32(actionInBytes, 0);
                
                var dataLengthInBytes = new byte[4];
                ReadDataFromStream(4, networkStream, dataLengthInBytes);
                var dataLength = BitConverter.ToInt32(dataLengthInBytes, 0);

                var dataBytes = new byte[dataLength];
                ReadDataFromStream(dataLength, networkStream, dataBytes);
                string data = Encoding.UTF8.GetString(dataBytes);

                route.dispatch(action, data, networkStream);
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
            return route.LoginAdmin(username, pass);
        }

        public bool addStudent(string studentUsername, string studentPass)
        {
            return route.addStudent(studentUsername, studentPass);
        }

        public List<string> getCousesAtString()
        {
            return route.getCousesAtString();
        }

        public void addCourse(string newCourse)
        {
            route.addCourse(newCourse);
        }

        public void removeCourse(int removeCourse)
        {
            route.removeCourse(removeCourse);
        }

        public List<string> getPosibleCoursesToAddTask()
        {
            return route.getPosibleCoursesToAddTask();
        }

        public void AddTask(string courseName, string taskName, int taskScore)
        {
            route.AddTask(courseName, taskName, taskScore);
        }

        public List<string> getCoursesWithTasks()
        {
            return route.getCoursesWithTasks();
        }
    }
}
