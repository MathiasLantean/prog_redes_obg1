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
        private static string coursesMenuPath = adminConsolePath + "\\Menus\\CousesMenu.txt";
        private static string tasksMenuPath = adminConsolePath + "\\Menus\\TaskMenu.txt";

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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Usuario o contraseña incorrectos, intente nuevamente.");
                        Console.ResetColor();
                    }
                }

                bool getOutOfAdminMenu = false;
                while (!getOutOfAdminMenu)
                {
                    showMenu(adminMenu);
                    int mainMenuOption = selectMenuOption(1, 3);
                    switch (mainMenuOption)
                    {
                        case 1:
                            Console.WriteLine("Ingrese el nombre del nuevo estudiante:");
                            string studentUsername = Console.ReadLine();
                            Console.WriteLine("Ingrese la contraseña del nuevo estudiante:");
                            string studentPass = Console.ReadLine();
                            if(server.addStudent(studentUsername, studentPass))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Estudiante agregado correctamente.");
                                Console.ResetColor();
                            }else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ya existe un estudiante con ese correo electrónico.");
                                Console.ResetColor();
                            }
                            Console.ReadLine();
                            break;
                        case 2:
                            bool getOutOfCoursesMenu = false;
                            while (!getOutOfCoursesMenu)
                            {
                                showMenu(coursesMenuPath);
                                int cousesMenuOption = selectMenuOption(1, 5);
                                switch (cousesMenuOption)
                                {
                                    case 1:
                                        List<string> coursesAtString = getCoursesListAtString();
                                        if (coursesAtString.Count > 0)
                                        {
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("No hay cursos disponibles para visualizar.");
                                            Console.ResetColor();
                                        }
                                        Console.ReadLine();
                                        break;
                                    case 2:
                                        getCoursesListAtString();
                                        Console.WriteLine();
                                        Console.Write("Ingrese el nombre del nuevo curso: ");
                                        string newCourse = Console.ReadLine();
                                        server.addCourse(newCourse);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("Curso agregado correctamente.");
                                        Console.ResetColor();
                                        Console.ReadLine();
                                        break;
                                    case 3:
                                        List<string> coursesToRemove = getCoursesListAtString();
                                        if (coursesToRemove.Count > 0)
                                        {
                                            Console.WriteLine();
                                            Console.Write("Ingrese la posición del curso a borrar: ");
                                            int removeCourse = selectMenuOption(1, coursesToRemove.Count);
                                            server.removeCourse(removeCourse - 1);
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("Curso removido correctamente.");
                                            Console.ResetColor();
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("No hay cursos disponibles para remover.");
                                            Console.ResetColor();
                                        }
                                        Console.ReadLine();
                                        break;
                                    case 4:
                                        bool getOutOfTaskMenu = false;
                                        while (!getOutOfTaskMenu)
                                        {
                                            showMenu(tasksMenuPath);
                                            int taskMenuOption = selectMenuOption(1, 4);
                                            switch (taskMenuOption)
                                            {
                                                case 1:
                                                    List<string> coursesWithTasks = getCoursesWithTasks();
                                                    if (coursesWithTasks.Count > 0)
                                                    {
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("No hay cursos con tareas asignadas.");
                                                        Console.ResetColor();
                                                    }
                                                    Console.ReadLine();
                                                    break;
                                                case 2:
                                                    List<string> coursesToAddTask = getPosibleCoursesToAddTask();
                                                    if (coursesToAddTask.Count > 0)
                                                    {
                                                        Console.WriteLine("Ingrese la posición del curso al que desea agregarle una tarea: ");
                                                        int courseToAddTask = selectMenuOption(1, coursesToAddTask.Count);
                                                        Console.WriteLine("Ingrese el nombre de la tarea: ");
                                                        string taskName = Console.ReadLine();
                                                        int maxTaskScore = Int32.Parse(coursesToAddTask[courseToAddTask - 1].Split('&')[1]);
                                                        int taskScore = 0;
                                                        while (!(taskScore > 0 && taskScore <= maxTaskScore))
                                                        {
                                                            Console.WriteLine("Ingrese la calificación máxima de la tarea (1-" + maxTaskScore + "): ");
                                                            try
                                                            {
                                                                taskScore = Int32.Parse(Console.ReadLine());
                                                            }
                                                            catch
                                                            {
                                                                Console.ForegroundColor = ConsoleColor.Red;
                                                                Console.WriteLine("El valor ingresador debe ser numérico.");
                                                                Console.ResetColor();
                                                            }
                                                        }
                                                        server.AddTask(coursesToAddTask[courseToAddTask - 1].Split('&')[0], taskName, taskScore);
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine("Tarea agregada correctamente.");
                                                        Console.ResetColor();
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("No hay cursos disponibles para agregar tareas.");
                                                        Console.ResetColor();
                                                    }
                                                    Console.ReadLine();
                                                    break;
                                                case 3:
                                                    List<string> coursesWithTasksToCorrect = getCoursesWithTasksToCorrect();
                                                    if (coursesWithTasksToCorrect.Count > 0)
                                                    {
                                                        Console.WriteLine("Ingrese la posición del curso del que desea corregir una tarea: ");
                                                        int courseToAddTask = selectMenuOption(1, coursesWithTasksToCorrect.Count);
                                                        string courseName = coursesWithTasksToCorrect[courseToAddTask-1].Split('&')[0];
                                                        List<string> tasksToCorrect = getTasksToCorrect(courseName);
                                                        Console.WriteLine("Ingrese la tarea que desea corregir: ");
                                                        int taskToCorrect = selectMenuOption(1, tasksToCorrect.Count);
                                                        string taskName = tasksToCorrect[taskToCorrect - 1].Split('[')[0].Remove(tasksToCorrect[taskToCorrect - 1].Split('[')[0].Count() - 1);
                                                        List<string> studentsToCorrect = getStudentsToCorrect(coursesWithTasksToCorrect[courseToAddTask - 1].Split('&')[0], taskName);
                                                        Console.WriteLine("Selecione al estudiante que desea calificar: ");
                                                        int studentToCorrect = selectMenuOption(1, studentsToCorrect.Count);
                                                        int studentNumber = Int32.Parse(studentsToCorrect[studentToCorrect - 1].Split(' ')[0]);
                                                        Console.WriteLine("Ingrese la nota(1-"+ tasksToCorrect[taskToCorrect - 1].Split('[')[1].Split(' ')[2].Split(']')[0] + "): ");
                                                        int score = selectMenuOption(1, Int32.Parse(tasksToCorrect[taskToCorrect - 1].Split('[')[1].Split(' ')[2].Split(']')[0]));
                                                        server.scoreStudent(courseName,taskName, studentNumber, score);
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine("Estudiante calificado con éxito.");
                                                        Console.ResetColor();
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("No hay cursos con tareas asignadas.");
                                                        Console.ResetColor();
                                                    }
                                                    Console.ReadLine();
                                                    break;
                                                case 4:
                                                    getOutOfTaskMenu = true;
                                                    break;
                                            }
                                        }
                                        break;
                                    case 5:
                                        getOutOfCoursesMenu = true;
                                        break;
                                }
                                
                            }
                            break;
                        case 3:
                            getOutOfAdminMenu = true;
                            break;
                    }
                }
            }
        }

        private static List<string> getStudentsToCorrect(string course, string task)
        {
            List<string> students = server.getStudentsToCorrect(course,task);
            for (int i = 0; i < students.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + students[i]);
            }
            return students;
        }

        private static List<string> getTasksToCorrect(string course)
        {
            List<string> tasks = server.getTasksToCorrect(course);
            for (int i = 0; i < tasks.Count; i++)
            {
                string task = tasks[i];
                Console.WriteLine((i+1) + ") " + task);
            }
            return tasks;
        }

        private static List<string> getCoursesWithTasks()
        {
            List<string> courses = server.getCoursesWithTasks();
            if (courses.Count > 0)
            {
                for (int i = 0; i < courses.Count; i++)
                {
                    string course = courses[i].Split('&')[0];
                    var tasks = courses[i].Split('&')[1].Split(',');
                    Console.WriteLine((i + 1) + ") " + course + ":");
                    foreach (string task in tasks)
                    {
                        Console.WriteLine("   -" + task);
                    }
                }
            }
            return courses;
        }

        private static List<string> getCoursesWithTasksToCorrect()
        {
            List<string> courses = server.getCoursesWithTasksToCorrect();
            if (courses.Count > 0)
            {
                for (int i = 0; i < courses.Count; i++)
                {
                    Console.WriteLine((i+1) + ") " + courses[i].Split('&')[0]);
                }
            }
            return courses;
        }

        private static List<string> getPosibleCoursesToAddTask()
        {
            List<string> courses = server.getPosibleCoursesToAddTask();
            for (int i = 0; i < courses.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + courses[i].Split('&')[0]);
            }
            return courses;
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Valor incorrecto, ingrese nuevamente.");
                    Console.ResetColor();
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
