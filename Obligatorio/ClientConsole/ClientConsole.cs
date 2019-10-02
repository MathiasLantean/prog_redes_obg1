using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLogic;
using System.IO;
using System.Threading;

namespace ClientConsole
{
    class ClientConsole
    {
        private static Client client = new Client();
        private static Client notifications = new Client();
        private static string clientConsolePath = System.Environment.CurrentDirectory.Remove(System.Environment.CurrentDirectory.Length - 9);
        private static string initMenuPath = clientConsolePath + "\\Menus\\InitMenu.txt";
        private static string sessionMenuPath = clientConsolePath + "\\Menus\\sessionMenu.txt";
        private static string mainMenuPath = clientConsolePath + "\\Menus\\mainMenu.txt";
        private static string coursesMenuPath = clientConsolePath + "\\Menus\\coursesMenu.txt";
        private static int studentLogged = 0;

        static void Main(string[] args)
        {
            Thread clientThread = new Thread(MainMenu);
            Thread notificationsThread = new Thread(GetNotifications);
            clientThread.Start();
            notificationsThread.Start();
        }

        private static void MainMenu()
        {
            while (true)
            {
                showMenu(initMenuPath, 0);
                Console.ReadLine();
                Console.WriteLine("Estableciendo conexión con el servidor...");
                client.Connect();
                Console.WriteLine("Conexión establecida.\n");
                bool getOutOfSessionMenu = false;
                while (!getOutOfSessionMenu)
                {
                    showMenu(sessionMenuPath, 0);
                    int menuOption = selectMenuOption(1, 2);
                    switch (menuOption)
                    {
                        case 1:
                            bool connPass = false;
                            while (!connPass)
                            {
                                Console.Write("Ingresar número de estudiante o correo electrónico: ");
                                string user = Console.ReadLine();
                                Console.Write("Ingresar constraseña: ");
                                string password = Console.ReadLine();
                                var loginResponse = client.Login(user, password);
                                if (loginResponse.Split('&')[0] == "T")
                                {
                                    connPass = true;
                                    studentLogged = Int32.Parse(loginResponse.Split('&')[1]);
                                }
                                if (!connPass)
                                {
                                    if (loginResponse.Split('&')[0] == "L")
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ya has iniciado sesión.");
                                        Console.ResetColor();
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Usuario o contraseña incorrectos, intente nuevamente.");
                                        Console.ResetColor();
                                    }

                                }
                            }
                            bool getOutOfMainMenu = false;
                            while (!getOutOfMainMenu)
                            {
                                showMenu(mainMenuPath, studentLogged);
                                int mainMenuOption = selectMenuOption(1, 3);
                                switch (mainMenuOption)
                                {
                                    case 1:
                                        bool getOutOfCoursesMenu = false;
                                        while (!getOutOfCoursesMenu)
                                        {
                                            showMenu(coursesMenuPath, studentLogged);
                                            int coursesMenuOption = selectMenuOption(1, 5);
                                            switch (coursesMenuOption)
                                            {
                                                case 1:
                                                    var courses = client.GetCourses(studentLogged);
                                                    if (!courses.Split(',')[0].Equals(""))
                                                    {
                                                        showList(courses);
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("No tienes cursos disponibles para mostrar.");
                                                        Console.ResetColor();
                                                    }
                                                    Console.ReadLine();
                                                    break;
                                                case 2:
                                                    var notSuscribedCourses = client.GetNotSuscribedCourses(studentLogged);
                                                    if (!notSuscribedCourses.Split(',')[0].Equals(""))
                                                    {
                                                        showList(notSuscribedCourses);
                                                        Console.WriteLine("Seleccione el curso al cual desea inscribirse.");
                                                        int selectedCourse = selectMenuOption(1, notSuscribedCourses.Split(',').Length);
                                                        client.Suscribe(notSuscribedCourses.Split(',')[selectedCourse - 1] + "&" + studentLogged);
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine("Te inscribiste al curso correctamente.");
                                                        Console.ResetColor();
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("No tienes cursos disponibles para inscribirte.");
                                                        Console.ResetColor();
                                                    }
                                                    Console.ReadLine();
                                                    break;
                                                case 3:
                                                    var suscribedCourses = client.GetSuscribedCourses(studentLogged);
                                                    if (!suscribedCourses.Split(',')[0].Equals(""))
                                                    {
                                                        showList(suscribedCourses);
                                                        Console.WriteLine("Seleccione el curso del cual desea darse de baja.");
                                                        int selectedCourse = selectMenuOption(1, suscribedCourses.Split(',').Length);
                                                        client.Unsuscribe(suscribedCourses.Split(',')[selectedCourse - 1] + "&" + studentLogged);
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine("Te diste de baja del curso correctamente.");
                                                        Console.ResetColor();
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("No tienes cursos disponibles para darte de baja.");
                                                        Console.ResetColor();
                                                    }
                                                    Console.ReadLine();
                                                    break;
                                                case 4:
                                                    var suscribedCoursesWithTasks = client.GetSuscribedCoursesWithTasks(studentLogged);
                                                    if (!suscribedCoursesWithTasks.Split(',')[0].Equals(""))
                                                    {
                                                        showList(suscribedCoursesWithTasks);
                                                        Console.WriteLine("Seleccione el curso al cual desea subirle una tarea.");
                                                        int selectedCourse = selectMenuOption(1, suscribedCoursesWithTasks.Split(',').Length);
                                                        var courseTasks = client.GetCourseTasks(suscribedCoursesWithTasks.Split(',')[selectedCourse - 1]);
                                                        showList(courseTasks);
                                                        Console.WriteLine("Seleccione la tarea que desea subir.");
                                                        int selectedTask = selectMenuOption(1, courseTasks.Split(',').Length);
                                                        Console.WriteLine("Ingrese la ubicación de la tarea.");
                                                        string taskPath = Console.ReadLine();
                                                        bool isValid = false;
                                                        while (!isValid)
                                                        {
                                                            try
                                                            {
                                                                if (Path.IsPathRooted(taskPath))
                                                                {
                                                                    isValid = true;
                                                                }
                                                                else
                                                                {
                                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                                    Console.WriteLine("Path inválido (el path debe estar sin comillas).");
                                                                    Console.ResetColor();
                                                                    taskPath = Console.ReadLine();
                                                                }
                                                            }catch {
                                                                Console.ForegroundColor = ConsoleColor.Red;
                                                                Console.WriteLine("Path inválido (el path debe estar sin comillas).");
                                                                Console.ResetColor();
                                                                taskPath = Console.ReadLine();
                                                            }
                                                        }
                                                        var taskUpdated = client.UpdateTaskToCourse(suscribedCoursesWithTasks.Split(',')[selectedCourse - 1], courseTasks.Split(',')[selectedTask - 1], taskPath, studentLogged);
                                                        if (taskUpdated.Equals("T"))
                                                        {
                                                            Console.ForegroundColor = ConsoleColor.Green;
                                                            Console.WriteLine("Has subido la tarea correctamente.");
                                                            Console.ResetColor();
                                                        }
                                                        else
                                                        {
                                                            Console.ForegroundColor = ConsoleColor.Red;
                                                            Console.WriteLine("Ha ocurrido un error al subir la tarea, intente nuevamente.");
                                                            Console.ResetColor();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("No tienes cursos con tareas disponibles.");
                                                        Console.ResetColor();
                                                    }
                                                    Console.ReadLine();
                                                    break;
                                                case 5:
                                                    getOutOfCoursesMenu = true;
                                                    break;
                                            }
                                        }
                                        break;
                                    case 2:
                                        var califications = client.GetCalifications(studentLogged);
                                        if (!califications.Equals(""))
                                        {
                                            string[] courses = califications.Split('$');
                                            foreach (string course in courses)
                                            {
                                                string courseCalification = course.Split('&')[0];
                                                Console.WriteLine(courseCalification);
                                                try
                                                {
                                                    string[] tasks = course.Split('&')[1].Split(';');
                                                    foreach (string task in tasks)
                                                    {
                                                        Console.WriteLine("   " + task);
                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("No tienes calificaciones disponbiles para ver.");
                                            Console.ResetColor();
                                        }
                                        Console.ReadLine();
                                        break;
                                    case 3:
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\nCerrando sesión...");
                                        Console.WriteLine("Sesión cerrada.\n");
                                        Console.ResetColor();
                                        client.Logout(studentLogged);
                                        notifications.DisconnectNotifications();
                                        studentLogged = 0;
                                        getOutOfMainMenu = true;
                                        Console.ReadLine();
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

        private static void showList(string list)
        {
            var courses = list.Split(',');
            for(int i = 0; i < courses.Length; i++)
            {
                Console.WriteLine((i+1)+") " + courses[i]);
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Opción incorrecta, seleccione nuevamente.");
                    Console.ResetColor();
                }
            }
            return option;
        }

        private static void GetNotifications()
        {
            notifications.ConnectNotification();
            while (true)
            {
                while (studentLogged != 0)
                {
                    if (notifications.ExistStreamNotifications())
                    {
                        string notification = notifications.GetNotifications(studentLogged);

                        if (!notification.Split(';')[0].Equals(""))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            foreach (string notif in notification.Split(';'))
                            {
                                Console.WriteLine(notif);
                            }
                            Console.ResetColor();
                        }
                        Thread.Sleep(30000);
                    }
                }
            }
            
        }
        private static void showMenu(string menuPath, int student)
        {
            if (student == 0)
            {
                Console.Clear();
                Console.WriteLine(File.ReadAllText(menuPath));
            }
            else
            {
                Console.Clear();
                Console.WriteLine(File.ReadAllText(menuPath));
            }
        }


    }
}
