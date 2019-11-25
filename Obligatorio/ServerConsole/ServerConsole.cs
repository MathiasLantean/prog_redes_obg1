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
        private static bool _isServerRunning;
        private static string adminConsolePath = System.Environment.CurrentDirectory;
        private static string adminMenu = adminConsolePath + "\\Menus\\adminMenu.txt";
        private static string initMenuPath = adminConsolePath + "\\Menus\\InitMenu.txt";
        private static string coursesMenuPath = adminConsolePath + "\\Menus\\CousesMenu.txt";
        private static string tasksMenuPath = adminConsolePath + "\\Menus\\TaskMenu.txt";

        public static async Task Main(string[] args)
        {
            _isServerRunning = true;

            await Task.Run(() => startServer().ConfigureAwait(false));

            await mainManuAdmin().ConfigureAwait(false);
            Console.Read();
        }

        private static async Task startServer()
        {
            try
            {
                await Task.Run(() => server.Start(_isServerRunning).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }

        private static async Task mainManuAdmin()
        {
            while (true)
            {
                await showMenu(initMenuPath).ConfigureAwait(false);
                Console.ReadLine();
                string username;
                string pass;
                bool connPass = false;
                Guid adminToken = Guid.Empty;
                while (!connPass)
                {
                    Console.Write("Ingresar nombre usuario: ");
                    username = Console.ReadLine();
                    Console.Write("Ingresar constraseña: ");
                    pass = Console.ReadLine();
                    adminToken = server.LoginAdmin(username, pass);
                    connPass = !adminToken.Equals(Guid.Empty); 
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
                    await showMenu(adminMenu).ConfigureAwait(false);
                    int mainMenuOption = await selectMenuOption(1, 3).ConfigureAwait(false);
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
                                await showMenu(coursesMenuPath).ConfigureAwait(false);
                                int cousesMenuOption = await selectMenuOption(1, 5).ConfigureAwait(false);
                                switch (cousesMenuOption)
                                {
                                    case 1:
                                        List<string> coursesAtString = await getCoursesListAtString().ConfigureAwait(false);
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
                                        await getCoursesListAtString().ConfigureAwait(false);
                                        Console.WriteLine();
                                        Console.Write("Ingrese el nombre del nuevo curso: ");
                                        string newCourse = Console.ReadLine();
                                        if (server.addCourse(newCourse))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("Curso agregado correctamente.");
                                            Console.ResetColor();
                                        }else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Ya existe un curso con ese nombre.");
                                            Console.ResetColor();
                                        }
                                        Console.ReadLine();
                                        break;
                                    case 3:
                                        List<string> coursesToRemove = await getCoursesListAtString().ConfigureAwait(false);
                                        if (coursesToRemove.Count > 0)
                                        {
                                            Console.WriteLine();
                                            Console.Write("Ingrese la posición del curso a borrar: ");
                                            int removeCourse = await selectMenuOption(1, coursesToRemove.Count).ConfigureAwait(false);
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
                                            await showMenu(tasksMenuPath).ConfigureAwait(false);
                                            int taskMenuOption = await selectMenuOption(1, 4).ConfigureAwait(false);
                                            switch (taskMenuOption)
                                            {
                                                case 1:
                                                    List<string> coursesWithTasks = await getCoursesWithTasks().ConfigureAwait(false);
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
                                                    List<string> coursesToAddTask = await getPosibleCoursesToAddTask().ConfigureAwait(false);
                                                    if (coursesToAddTask.Count > 0)
                                                    {
                                                        Console.WriteLine("Ingrese la posición del curso al que desea agregarle una tarea: ");
                                                        int courseToAddTask = await selectMenuOption(1, coursesToAddTask.Count).ConfigureAwait(false);
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
                                                    List<string> coursesWithTasksToCorrect = await getCoursesWithTasksToCorrect(adminToken).ConfigureAwait(false);
                                                    if (coursesWithTasksToCorrect.Count > 0)
                                                    {
                                                        Console.WriteLine("Ingrese la posición del curso del que desea corregir una tarea: ");
                                                        int courseToAddTask = await selectMenuOption(1, coursesWithTasksToCorrect.Count).ConfigureAwait(false);
                                                        string courseName = coursesWithTasksToCorrect[courseToAddTask-1].Split('&')[0];
                                                        List<string> tasksToCorrect = await getTasksToCorrect(courseName).ConfigureAwait(false);
                                                        Console.WriteLine("Ingrese la tarea que desea corregir: ");
                                                        int taskToCorrect = await selectMenuOption(1, tasksToCorrect.Count).ConfigureAwait(false);
                                                        string taskName = tasksToCorrect[taskToCorrect - 1].Split('[')[0].Remove(tasksToCorrect[taskToCorrect - 1].Split('[')[0].Count() - 1);
                                                        List<string> studentsToCorrect = await getStudentsToCorrect(coursesWithTasksToCorrect[courseToAddTask - 1].Split('&')[0], taskName).ConfigureAwait(false);
                                                        Console.WriteLine("Selecione al estudiante que desea calificar: ");
                                                        int studentToCorrect = await selectMenuOption(1, studentsToCorrect.Count).ConfigureAwait(false);
                                                        int studentNumber = Int32.Parse(studentsToCorrect[studentToCorrect - 1].Split(' ')[0]);
                                                        Console.WriteLine("Ingrese la nota(1-"+ tasksToCorrect[taskToCorrect - 1].Split('[')[1].Split(' ')[2].Split(']')[0] + "): ");
                                                        int score = await selectMenuOption(1, Int32.Parse(tasksToCorrect[taskToCorrect - 1].Split('[')[1].Split(' ')[2].Split(']')[0])).ConfigureAwait(false);
                                                        server.ScoreStudent(adminToken,courseName, taskName, studentNumber, score);
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

        private static async Task<List<string>> getStudentsToCorrect(string course, string task)
        {
            List<string> students = server.GetStudentsToCorrect(course,task);
            for (int i = 0; i < students.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + students[i]);
            }
            return students;
        }

        private static async Task<List<string>> getTasksToCorrect(string course)
        {
            List<string> tasks = server.GetTasksToCorrect(course);
            for (int i = 0; i < tasks.Count; i++)
            {
                string task = tasks[i];
                Console.WriteLine((i+1) + ") " + task);
            }
            return tasks;
        }

        private static async Task<List<string>> getCoursesWithTasks()
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

        private static async Task<List<string>> getCoursesWithTasksToCorrect(Guid adminToken)
        {
            List<string> courses = server.GetCoursesWithTasksToCorrect(adminToken);
            if (courses.Count > 0)
            {
                for (int i = 0; i < courses.Count; i++)
                {
                    Console.WriteLine((i+1) + ") " + courses[i].Split('&')[0]);
                }
            }
            return courses;
        }

        private static async Task<List<string>> getPosibleCoursesToAddTask()
        {
            List<string> courses = server.getPosibleCoursesToAddTask();
            for (int i = 0; i < courses.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + courses[i].Split('&')[0]);
            }
            return courses;
        }

        private static async Task<List<string>> getCoursesListAtString()
        {
            List<string> courses = server.getCousesAtString();
            for (int i = 0; i < courses.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + courses[i].ToString());
            }
            return courses;
        }

        private static async Task<int> selectMenuOption(int minOption, int maxOption)
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

        private static async Task showMenu(string menuPath)
        {
            Console.Clear();
            Console.WriteLine(File.ReadAllText(menuPath));
        }
    }
}
