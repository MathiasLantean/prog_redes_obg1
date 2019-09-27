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
using System.Web.Script.Serialization;

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
            var payload = new Dictionary<string, string>(){
                        { "Username", user },
                        { "Password", password }
                };
            sendData(Action.Login, payload, this.stream);

            var response = new byte[4];
            ReadDataFromStream(4, stream, response);
            if (BitConverter.ToInt32(response, 0) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void sendData(Action action, Dictionary<string, string> payload, NetworkStream networkStream)
        {
            var actionInBit = BitConverter.GetBytes((int)action);
            var jsonString = new JavaScriptSerializer().Serialize(payload);
            var messageInBytes = Encoding.UTF8.GetBytes(jsonString);
            var lengthOfDataInBytes = BitConverter.GetBytes(messageInBytes.Length);

            networkStream.Write(actionInBit, 0, actionInBit.Length);
            networkStream.Write(lengthOfDataInBytes, 0, lengthOfDataInBytes.Length);
            networkStream.Write(messageInBytes, 0, messageInBytes.Length);
        }

        private string reciveData()
        {
            var action = new byte[4];
            ReadDataFromStream(4, stream, action);
            var responseLength = new byte[4];
            ReadDataFromStream(4, stream, responseLength);
            var responseInBytes = new byte[BitConverter.ToInt32(responseLength, 0)];
            ReadDataFromStream(responseInBytes.Length, stream, responseInBytes);
            string data = Encoding.UTF8.GetString(responseInBytes);
            return data;
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

        public string GetCourses()
        {
            sendData(Action.GetCourses, new Dictionary<string, string> { }, this.stream);
            return reciveData();
        }
    }
}
