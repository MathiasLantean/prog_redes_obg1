using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLogic;
using ServerLogic;
using System.IO;

namespace ClientConsole
{
    class ClientConsole
    {
        private static Client client = new Client();
        private static string clientConsolePath = System.Environment.CurrentDirectory.Remove(System.Environment.CurrentDirectory.Length - 9);
        private static string initMenuPath = clientConsolePath + "\\Menus\\InitMenu.txt";
        private static string sessionMenuPath = clientConsolePath + "\\Menus\\sessionMenu.txt";
        private static string mainMenuPath = clientConsolePath + "\\Menus\\mainMenu.txt";

        static void Main(string[] args)
        {
            while (true)
            {
                showMenu(initMenuPath);
                Console.ReadLine();
                Console.WriteLine("Estableciendo conexión con el servidor...");
                client.Connect();
                Console.WriteLine("Conexión establecida.\n");
                bool getOutOfSessionMenu = false;
                while (!getOutOfSessionMenu) {
                    showMenu(sessionMenuPath);
                    int menuOption = selectMenuOption(1, 2);
                    switch (menuOption)
                    {
                        case 1:
                            bool connPass = false;
                            while (!connPass)
                            {
                                Console.Write("Ingresar nombre usuario: ");
                                string user = Console.ReadLine();
                                Console.Write("Ingresar constraseña: ");
                                string password = Console.ReadLine();
                                connPass = true;//connPass = server.Login(user, password);
                                if (!connPass)
                                {
                                    Console.WriteLine("Usuario o contraseña incorrectos, intente nuevamente.");
                                }
                            }
                            bool getOutOfMainMenu = false;
                            while (!getOutOfMainMenu) {
                                showMenu(mainMenuPath);
                                int mainMenuOption = selectMenuOption(1, 4);
                                switch (mainMenuOption)
                                {
                                    case 1:
                                        break;
                                    case 2:
                                        break;
                                    case 3:
                                        break;
                                    case 4:
                                        Console.WriteLine("\nCerrando sesión...");
                                        Console.WriteLine("Sesión cerrada.\n");
                                        getOutOfMainMenu = true;
                                        break;
                                }
                            }
                            break;
                        case 2:
                            client.Disconnect();
                            getOutOfSessionMenu = true;
                            break;
                    }
                }
            }
        }


        private static int selectMenuOption(int minOption, int maxOption)
        {
            int option = 0;
            while(!(option >= minOption && option <= maxOption)) {
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
