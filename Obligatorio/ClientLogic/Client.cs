using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Action = RouteController.Action;
using System.IO;
using System.Threading.Tasks;
using Communication.Protocol;
using Communication.TcpSockets;


namespace ClientLogic
{
    public class Client
    {
        private WriteTcpSockets stream;
        private ReadTcpSockets streamReader;
        private TcpClient tcpClient;
        private WriteTcpSockets streamNotifications;
        private ReadTcpSockets streamReaderNotifications;
        private TcpClient tcpClientNotifications;
        public async Task Connect()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                IPAddress serverIpAddress = IPAddress.Parse(appSettings["ServerIpAddress"]);
                int serverPort = Int32.Parse(appSettings["ServerPort"]);
                IPAddress clientIpAddress = IPAddress.Parse(appSettings["ClientIpAddress"]);
                int clientPort = Int32.Parse(appSettings["ClientPort"]);

                var tcpListener = new TcpListener(serverIpAddress, serverPort);

                this.tcpClient = new TcpClient(new IPEndPoint(clientIpAddress, clientPort));
                await tcpClient.ConnectAsync(serverIpAddress, serverPort).ConfigureAwait(false);

                this.stream = new WriteTcpSockets(tcpClient);
                this.streamReader = new ReadTcpSockets(tcpClient);
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

        public async Task ConnectNotification()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                IPAddress serverIpAddress = IPAddress.Parse(appSettings["ServerIpAddress"]);
                int serverPort = Int32.Parse(appSettings["ServerPort"]);
                IPAddress clientIpAddress = IPAddress.Parse(appSettings["ClientIpAddress"]);
                int clientPort = Int32.Parse(appSettings["ClientPort"]);

                var tcpListener = new TcpListener(serverIpAddress, serverPort);

                this.tcpClientNotifications = new TcpClient(new IPEndPoint(clientIpAddress, clientPort));
                await tcpClientNotifications.ConnectAsync(serverIpAddress, serverPort).ConfigureAwait(false);

                this.streamNotifications = new WriteTcpSockets(tcpClientNotifications);
                this.streamReaderNotifications = new ReadTcpSockets(tcpClientNotifications);
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

        public void Disconnect()
        {
            if (!tcpClient.Connected) return;
            tcpClient.GetStream().Close();
            tcpClient.Close();
        }

        public void DisconnectNotifications()
        {
            this.tcpClientNotifications.Close();
            this.tcpClientNotifications.GetStream().Close();
        }

        public async Task<string> Login(string user, string password)
        {
            var payload = user + "&" + password;
            await sendData(Action.Login, payload, this.stream).ConfigureAwait(false);

            return await reciveData(this.streamReader).ConfigureAwait(false);

        }

        public async Task<string> GetCourses(int studentNumber)
        {
            await sendData(Action.GetCourses, studentNumber.ToString(), this.stream).ConfigureAwait(false);
            return await reciveData(this.streamReader).ConfigureAwait(false);
        }

        public async Task Suscribe(string cursonumeroestudiante)
        {
            await sendData(Action.Suscribe, cursonumeroestudiante, this.stream).ConfigureAwait(false);
        }

        private async Task sendData(Action action, string payload, WriteTcpSockets networkStream)
        {
            var actionInBit = BitConverter.GetBytes((int)action);
            var messageInBytes = Encoding.UTF8.GetBytes(payload);
            var lengthOfDataInBytes = BitConverter.GetBytes(messageInBytes.Length);

            await networkStream.WriteDataAsync(actionInBit).ConfigureAwait(false); ;
            await networkStream.WriteDataAsync(lengthOfDataInBytes).ConfigureAwait(false);
            await networkStream.WriteDataAsync(messageInBytes).ConfigureAwait(false);
        }

        private async Task<string> reciveData(ReadTcpSockets stream)
        {
            try
            {
                var actionInBytes = await stream.ReadDataAsync(ProtocolConstants.FixedLength).ConfigureAwait(false);
                Action action = (Action)BitConverter.ToInt32(actionInBytes, 0);
                var dataLengthInBytes = await stream.ReadDataAsync(ProtocolConstants.FixedLength).ConfigureAwait(false);
                var dataLength = BitConverter.ToInt32(dataLengthInBytes, 0);
                var responseInBytes = await stream.ReadDataAsync(dataLength).ConfigureAwait(false);
                var data = Encoding.UTF8.GetString(responseInBytes);

                return data;
            }
            catch
            {
                return "";
            }
        }

        public async Task<string> GetNotSuscribedCourses(int studentNumber)
        {
            await sendData(Action.GetNotSuscribedCourses, studentNumber.ToString(), this.stream).ConfigureAwait(false);
            return await reciveData(this.streamReader).ConfigureAwait(false);
        }

        public async Task<string> GetSuscribedCourses(int studentNumber)
        {
            await sendData(Action.GetSuscribedCourses, studentNumber.ToString(), this.stream).ConfigureAwait(false);
            return await reciveData(this.streamReader).ConfigureAwait(false);
        }

        public async Task Unsuscribe(string cursonumeroestudiante)
        {
            await sendData(Action.Unsuscribe, cursonumeroestudiante, stream).ConfigureAwait(false);
        }

        public async Task<string> GetSuscribedCoursesWithTasks(int studentNumber)
        {
            await sendData(Action.GetSuscribedCoursesWithTasks, studentNumber.ToString(), this.stream).ConfigureAwait(false);
            return await reciveData(this.streamReader).ConfigureAwait(false);
        }

        public async Task<string> GetCourseTasks(string course)
        {
            await sendData(Action.GetCourseTasks, course, this.stream).ConfigureAwait(false);
            return await reciveData(this.streamReader).ConfigureAwait(false);
        }

        public async Task<string> UpdateTaskToCourse(string courseName, string taskName, string taskPath, int studentNumber)
        {
            string extension = Path.GetExtension(taskPath);

            using (FileStream fs = new FileStream(taskPath, FileMode.Open, FileAccess.Read))
            {
                byte[] fileInBytes = new byte[fs.Length];
                await fs.ReadAsync(fileInBytes, 0, fileInBytes.Length).ConfigureAwait(false);
                string taskFile = Convert.ToBase64String(fileInBytes);
                await sendData(Action.UpdateTaskToCourse, courseName + "&" + taskName + "&" + studentNumber + "&" + extension + "&" + taskFile, this.stream).ConfigureAwait(false);
            }
            
            return await reciveData(this.streamReader).ConfigureAwait(false);
        }

        public async Task<string> GetNotifications(int student)
        {
            await sendData(Action.GetNotifications, student.ToString(), this.streamNotifications).ConfigureAwait(false);
            return await reciveData(this.streamReaderNotifications).ConfigureAwait(false); 
        }

        public async Task Logout(int studentNumber)
        {
            await sendData(Action.Logout, studentNumber.ToString(), this.stream).ConfigureAwait(false);
        }

        public async Task<string> GetCalifications(int studentNumber)
        {
            await sendData(Action.GetCalifications, studentNumber.ToString(), this.stream).ConfigureAwait(false);
            return await reciveData(this.streamReader).ConfigureAwait(false);
        }

        public bool ExistStreamNotifications()
        {
            return this.streamNotifications != null;
        }
    }
}
