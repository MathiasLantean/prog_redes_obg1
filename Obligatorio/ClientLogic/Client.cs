using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Domain;
using ContentFactory;

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
            //Content data = new JSon(Action.Login, ContetType.Lista, );
            //string info = "{Username: " + user;
            //data.Add();
            //sendDataToServer(Content dataToSend, ContentType.Text);
            return true;
        }

        private void sendDataToServer(Content data, ContetType dataType)
        {
            //stream.Write(data.LengthInBytes(dataType));
            //stream.Write(data.DataInBytes(dataType));
            throw new NotImplementedException();
        }
    }
}
