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
        private static string cousesMenuPath = adminConsolePath + "\\Menus\\CousesMenu.txt";

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
                            bool getOutOfCoursesMenu = false;
                            while (!getOutOfCoursesMenu)
                            {
                                showMenu(cousesMenuPath);
                                int cousesMenuOption = selectMenuOption(1, 4);
                                switch (cousesMenuOption)
                                {
                                    case 1:
                                        getCoursesListAtString();
                                        Console.WriteLine();
                                        Console.WriteLine("Presione ENTER para continuar.");
                                        Console.ReadLine();
                                        break;
                                    case 2:
                                        getCoursesListAtString();
                                        Console.WriteLine();
                                        Console.Write("Ingrese el nombre del nuevo curso: ");
                                        string newCourse = Console.ReadLine();
                                        server.addCourse(newCourse);
                                        break;
                                    case 3:
                                        List<string> coursesToRemove = getCoursesListAtString();
                                        Console.WriteLine();
                                        Console.Write("Ingrese la posición del curso a borrar: ");
                                        int removeCourse = selectMenuOption(1, coursesToRemove.Count);
                                        server.removeCourse(removeCourse-1);
                                        break;
                                    case 4:
                                        getOutOfCoursesMenu = true;
                                        break;
                                }
                                
                            }
                            break;
                        case 3:
                            break;
                        case 4:
                            getOutOfAdminMenu = true;
                            break;
                    }
                }
            }
        }

        private static List<string> getCoursesListAtString()
        {
            List<string> courses = server.getCousesAtString();
            for (int i = 0; i < courses.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + courses[i].ToString());
            }
            return courses;
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
