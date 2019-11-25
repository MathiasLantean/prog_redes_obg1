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
        private static string clientConsolePath = System.Environment.CurrentDirectory;
        private static string initMenuPath = clientConsolePath + "\\Menus\\InitMenu.txt";
        private static string sessionMenuPath = clientConsolePath + "\\Menus\\sessionMenu.txt";
        private static string mainMenuPath = clientConsolePath + "\\Menus\\mainMenu.txt";
        private static string coursesMenuPath = clientConsolePath + "\\Menus\\coursesMenu.txt";
        private static int studentLogged = 0;

        static async Task Main(string[] args)
        {
            try
            {
                await MainMenu().ConfigureAwait(false);
            }catch(Exception e)
            {

            }
        }

        private static async Task MainMenu()
        {
            var sendData = true;
            while (sendData)
            {
                await showMenu(initMenuPath, 0).ConfigureAwait(false);
                Console.ReadLine();
                Console.WriteLine("Estableciendo conexión con el servidor...");
                await client.Connect().ConfigureAwait(false);
                Console.WriteLine("Conexión establecida.\n");
                await Task.Run(() => GetNotifications().ConfigureAwait(false)).ConfigureAwait(false);
                bool getOutOfSessionMenu = false;
                while (!getOutOfSessionMenu)
                {
                    await showMenu(sessionMenuPath, 0).ConfigureAwait(false);
                    int menuOption = await selectMenuOption(1, 2).ConfigureAwait(false);
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
                                var loginResponse = await client.Login(user, password).ConfigureAwait(false);
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
                                await showMenu(mainMenuPath, studentLogged).ConfigureAwait(false);
                                int mainMenuOption = await selectMenuOption(1, 3).ConfigureAwait(false);
                                switch (mainMenuOption)
                                {
                                    case 1:
                                        bool getOutOfCoursesMenu = false;
                                        while (!getOutOfCoursesMenu)
                                        {
                                            await showMenu(coursesMenuPath, studentLogged).ConfigureAwait(false);
                                            int coursesMenuOption = await selectMenuOption(1, 5).ConfigureAwait(false);
                                            switch (coursesMenuOption)
                                            {
                                                case 1:
                                                    var courses = await client.GetCourses(studentLogged).ConfigureAwait(false);
                                                    if (!courses.Split(',')[0].Equals(""))
                                                    {
                                                        await showList(courses).ConfigureAwait(false); ;
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
                                                    var notSuscribedCourses = await client.GetNotSuscribedCourses(studentLogged).ConfigureAwait(false);
                                                    if (!notSuscribedCourses.Split(',')[0].Equals(""))
                                                    {
                                                        await showList(notSuscribedCourses).ConfigureAwait(false); ;
                                                        Console.WriteLine("Seleccione el curso al cual desea inscribirse.");
                                                        int selectedCourse = await selectMenuOption(1, notSuscribedCourses.Split(',').Length).ConfigureAwait(false);
                                                        await client.Suscribe(notSuscribedCourses.Split(',')[selectedCourse - 1] + "&" + studentLogged);
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
                                                    var suscribedCourses = await client.GetSuscribedCourses(studentLogged).ConfigureAwait(false);
                                                    if (!suscribedCourses.Split(',')[0].Equals(""))
                                                    {
                                                        await showList(suscribedCourses).ConfigureAwait(false);
                                                        Console.WriteLine("Seleccione el curso del cual desea darse de baja.");
                                                        int selectedCourse = await selectMenuOption(1, suscribedCourses.Split(',').Length).ConfigureAwait(false);
                                                        await client.Unsuscribe(suscribedCourses.Split(',')[selectedCourse - 1] + "&" + studentLogged);
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
                                                    var suscribedCoursesWithTasks = await client.GetSuscribedCoursesWithTasks(studentLogged).ConfigureAwait(false);
                                                    if (!suscribedCoursesWithTasks.Split(',')[0].Equals(""))
                                                    {
                                                        await showList(suscribedCoursesWithTasks).ConfigureAwait(false);
                                                        Console.WriteLine("Seleccione el curso al cual desea subirle una tarea.");
                                                        int selectedCourse = await selectMenuOption(1, suscribedCoursesWithTasks.Split(',').Length).ConfigureAwait(false);
                                                        var courseTasks = await client.GetCourseTasks(suscribedCoursesWithTasks.Split(',')[selectedCourse - 1]).ConfigureAwait(false);
                                                        await showList(courseTasks).ConfigureAwait(false);
                                                        Console.WriteLine("Seleccione la tarea que desea subir.");
                                                        int selectedTask = await selectMenuOption(1, courseTasks.Split(',').Length).ConfigureAwait(false);
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
                                                        var taskUpdated = await client.UpdateTaskToCourse(suscribedCoursesWithTasks.Split(',')[selectedCourse - 1], courseTasks.Split(',')[selectedTask - 1], taskPath, studentLogged);
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
                                        var califications = await client.GetCalifications(studentLogged).ConfigureAwait(false);
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
                                        await client.Logout(studentLogged).ConfigureAwait(false);
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
                            sendData = false;
                            break;
                    }
                }
            }
        }

        private static async Task showList(string list)
        {
            var courses = list.Split(',');
            for(int i = 0; i < courses.Length; i++)
            {
                Console.WriteLine((i+1)+") " + courses[i]);
            }
        }

        private static async Task<int> selectMenuOption(int minOption, int maxOption)
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

        private static async Task  GetNotifications()
        {
            await notifications.ConnectNotification().ConfigureAwait(false);
            while (true)
            {
                while (studentLogged != 0)
                {
                    if (notifications.ExistStreamNotifications())
                    {
                        string notification = await notifications.GetNotifications(studentLogged).ConfigureAwait(false);

                        if (!notification.Split(';')[0].Equals(""))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            foreach (string notif in notification.Split(';'))
                            {
                                Console.WriteLine(notif);
                            }
                            Console.ResetColor();
                        }
                        Thread.Sleep(5000);
                    }
                }
            }
            
        }
        private static async Task showMenu(string menuPath, int student)
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
