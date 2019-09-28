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
using System.IO;

namespace ClientLogic
{
    public class Client
    {
        private NetworkStream stream;
        private TcpClient tcpClient;
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

        public string Login(string user, string password)
        {
            var payload = user + "&" + password;
            sendData(Action.Login, payload, this.stream);

            return reciveData();

        }

        public string GetCourses(int studentNumber)
        {
            sendData(Action.GetCourses, studentNumber.ToString(), this.stream);
            return reciveData();
        }

        public void Suscribe(string cursonumeroestudiante)
        {
            sendData(Action.Suscribe, cursonumeroestudiante, stream);
        }

        private void sendData(Action action, string payload, NetworkStream networkStream)
        {
            var actionInBit = BitConverter.GetBytes((int)action);
            var messageInBytes = Encoding.UTF8.GetBytes(payload);
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

        public string GetNotSuscribedCourses(int studentNumber)
        {
            sendData(Action.GetNotSuscribedCourses, studentNumber.ToString(), this.stream);
            return reciveData();
        }

        public string GetSuscribedCourses(int studentNumber)
        {
            sendData(Action.GetSuscribedCourses, studentNumber.ToString(), this.stream);
            return reciveData();
        }

        public void Unsuscribe(string cursonumeroestudiante)
        {
            sendData(Action.Unsuscribe, cursonumeroestudiante, stream);
        }

        public string GetSuscribedCoursesWithTasks(int studentNumber)
        {
            sendData(Action.GetSuscribedCoursesWithTasks, studentNumber.ToString(), this.stream);
            return reciveData();
        }

        public string GetCourseTasks(string course)
        {
            sendData(Action.GetCourseTasks, course, this.stream);
            return reciveData();
        }

        public string UpdateTaskToCourse(string courseName, string taskName, string taskPath, int studentNumber)
        {
            string extension = Path.GetExtension(taskPath);
            string taskFile = ACharToString(File.ReadAllBytes(taskPath).Select(x=>(char)x).ToArray());
            sendData(Action.UpdateTaskToCourse, courseName + "&" + taskName + "&" + studentNumber + "&"+ extension + "&" + taskFile, this.stream);
            return reciveData();
        }
        private string ACharToString(char[] arrChar)
        {
            string result = "";
            foreach (char character in arrChar)
            {
                result += character;
            }
            return result;
        }
    }
}
