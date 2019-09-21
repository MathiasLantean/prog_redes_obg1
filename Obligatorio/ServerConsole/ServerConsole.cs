using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerLogic;
using System.IO;

namespace ServerConsole
{
    class ServerConsole
    {
        private static Server server = new Server();
        private static string adminConsolePath = System.Environment.CurrentDirectory.Remove(System.Environment.CurrentDirectory.Length - 9);
        private static string adminMenu = adminConsolePath + "\\Menus\\adminMenu.txt";
        private static string initMenuPath = adminConsolePath + "\\Menus\\InitMenu.txt";

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
            while (true)
            {
                showMenu(initMenuPath);
                Console.ReadLine();
                string username;
                string pass;
                bool connPass = false;
                while (!connPass)
                {
                    Console.Write("Ingresar nombre usuario: ");
                    username = Console.ReadLine();
                    Console.Write("Ingresar constraseña: ");
                    pass = Console.ReadLine();
                    connPass = server.LoginAdmin(username, pass);
                    if (!connPass)
                    {
                        Console.WriteLine("Usuario o contraseña incorrectos, intente nuevamente.");
                    }
                }

                bool getOutOfAdminMenu = false;
                while (!getOutOfAdminMenu)
                {
                    showMenu(adminMenu);
                    int mainMenuOption = selectMenuOption(1, 4);
                    switch (mainMenuOption)
                    {
                        case 1:
                            Console.WriteLine("Ingrese el nombre del nuevo estudiante:");
                            string studentUsername = Console.ReadLine();
                            Console.WriteLine("Ingrese la contraseña del nuevo estudiante:");
                            string studentPass = Console.ReadLine();
                            server.addStudent(studentUsername, studentPass);
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                        case 4:
                            Console.WriteLine("\nCerrando sesión...");
                            Console.WriteLine("Sesión cerrada.\n");
                            getOutOfAdminMenu = true;
                            break;
                    }
                }
            }
        }

        private static int selectMenuOption(int minOption, int maxOption)
        {
            int option = 0;
            while (!(option >= minOption && option <= maxOption))
            {
                try
                {
                    option = Int32.Parse(Console.ReadLine());
                }
                catch { option = 0; }
                if (!(option >= minOption && option <= maxOption))
                {
                    Console.WriteLine("Opción incorrecta, seleccione nuevamente.");
                }
            }
            return option;
        }

        private static void showMenu(string menuPath)
        {
            Console.Clear();
            Console.WriteLine(File.ReadAllText(menuPath));
        }
    }
}
