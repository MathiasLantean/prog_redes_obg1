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

namespace ClientLogic
{
    public class Client
    {
        private NetworkStream stream;
        private TcpClient tcpClient;
        private Student loggedStudent;
        public void Connect()
        {
            this.tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            tcpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6000));
            this.stream = tcpClient.GetStream();
        }

        public void Disconnect()
        {
            this.stream.Close();
            this.tcpClient.Close();
        }

        public bool Login(string user, string password)
        {
            var action = BitConverter.GetBytes((int)Action.Login);
            stream.Write(action, 0, action.Length);
            var typeContent = BitConverter.GetBytes((int)ContentType.Json);
            stream.Write(typeContent, 0, typeContent.Length);
            string json = "{ Username: " + user + ", Password: " + password + "}";
            var mensajeInBytes = Encoding.UTF8.GetBytes(json);
            var lengthOfData = mensajeInBytes.Length;
            var lengthOfDataInBytes = BitConverter.GetBytes(lengthOfData);
            stream.Write(lengthOfDataInBytes, 0, lengthOfDataInBytes.Length);
            stream.Write(mensajeInBytes, 0, mensajeInBytes.Length);

            var response = new byte[4];
            ReadDataFromStream(4, stream, response);
            if(BitConverter.ToInt32(response, 0) == 1)
            {
                return true;
            }else
            {
                return false;
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
    }
}
