using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerLogic;

namespace ServerConsole
{
    class ServerConsole
    {
        private static Server server = new Server();

        static void Main(string[] args)
        {
            var serverThread = new Thread(startServer);
            var adminThread = new Thread(mainManuAdmin);
            adminThread.Start();
            serverThread.Start();
            adminThread.Join();
            serverThread.Join();
        }

        private static void startServer()
        {
            server.Start();
        }

        private static void mainManuAdmin()
        {
            string username;
            string pass;
            do
            {
                Console.WriteLine("Username:");
                username = Console.ReadLine();
                Console.WriteLine("Password:");
                pass = Console.ReadLine();
            } while (!server.LoginAdmin(username, pass));
            Console.WriteLine("ADMIN MENU");
            Console.WriteLine("1-Registrar estudiante");
            Console.WriteLine("username:");
            string studentUsername = Console.ReadLine();
            Console.WriteLine("password:");
            string studentPass = Console.ReadLine();
            server.addStudent(studentUsername, studentPass);
        }
    }
}
