﻿using System;
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
            var action = BitConverter.GetBytes((int)Action.Login);
            var payload = new Dictionary<string, string>(){
                        { "Username", user },
                        { "Password", password }
                };
            var jsonString = new JavaScriptSerializer().Serialize(payload);
            var messageInBytes = Encoding.UTF8.GetBytes(jsonString);
            var lengthOfDataInBytes = BitConverter.GetBytes(messageInBytes.Length);
            sendData(action, messageInBytes, lengthOfDataInBytes);

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

        private void sendData(byte[] action, byte[] messageInBytes, byte[] lengthOfDataInBytes)
        {
            stream.Write(action, 0, action.Length);
            stream.Write(lengthOfDataInBytes, 0, lengthOfDataInBytes.Length);
            stream.Write(messageInBytes, 0, messageInBytes.Length);
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
