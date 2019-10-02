using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Action = RouteController.Action;
using RouteController;
using Domain;

namespace ServerLogic
{
    public class Server
    {

        //private ActionDispatcher route = new ActionDispatcher();
        private DataSystem data = DataSystem.Instance;

        public void Start()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                IPAddress serverIpAddress = IPAddress.Parse(appSettings["ServerIpAddress"]);
                int serverPort = Int32.Parse(appSettings["ServerPort"]);

                var tcpListener = new TcpListener(serverIpAddress, serverPort);
                tcpListener.Start(100);
                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();
                    ActionDispatcher route = new ActionDispatcher();
                    var thread = new Thread(() => actionDespatcher(tcpClient,route));
                    thread.Start();
                }
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

        private void actionDespatcher(TcpClient tcpClient, ActionDispatcher route)
        {
            var networkStream = tcpClient.GetStream();

            while (true)
            {
                var actionInBytes = new byte[4];
                ReadDataFromStream(4, networkStream, actionInBytes);
                Action action = (Action)BitConverter.ToInt32(actionInBytes, 0);
                
                var dataLengthInBytes = new byte[4];
                ReadDataFromStream(4, networkStream, dataLengthInBytes);
                var dataLength = BitConverter.ToInt32(dataLengthInBytes, 0);

                var dataBytes = new byte[dataLength];
                ReadDataFromStream(dataLength, networkStream, dataBytes);
                string data = Encoding.UTF8.GetString(dataBytes);

                route.dispatch(action, data, networkStream);
            }
        }
        private static void ReadDataFromStream(int length, NetworkStream networkStream, byte[] dataBytes)
        {
            var totalRecivedData = 0;
            while (totalRecivedData < length)
            {
                var recived = networkStream.Read(dataBytes, totalRecivedData, length - totalRecivedData);
                if (recived == 0)
                {
                    //ERROR
                }
                totalRecivedData = recived;
            }
            totalRecivedData = 0;
        }
        public bool LoginAdmin(string username, string pass)
        {
            User user = new User() { Email = username, Password = pass };
            Admin adminLog = new Admin() { User = user };
            return DataSystem.Instance.CheckAdminPassword(adminLog);
    }

        public bool addStudent(string studentUsername, string studentPass)
        {
            User studentUser = new User() { Email = studentUsername, Password = studentPass };
            Student studentToAdd = new Student() { User = studentUser};
            return DataSystem.Instance.AddStudent(studentToAdd);
        }

        public List<string> getCousesAtString()
        {
            List<string> coursesAtString = new List<string>();
            foreach (Course course in DataSystem.Instance.Courses)
            {
                coursesAtString.Add(course.ToString());
            }
            return coursesAtString;
        }

        public bool addCourse(string newCourse)
        {
            Course course = new Course() { Name = newCourse };
            return DataSystem.Instance.AddCourse(course);
        }

        public void removeCourse(int courseToRemoveIndex)
        {
            Course courseToRemove = DataSystem.Instance.Courses[courseToRemoveIndex];
            DataSystem.Instance.RemoveCourse(courseToRemove);
        }

        public List<string> getPosibleCoursesToAddTask()
        {
            List<string> coursesWithNote = new List<string>();
            foreach (Course course in DataSystem.Instance.Courses)
            {
                if (course.totalTaskScore < 100)
                {
                    coursesWithNote.Add(course.ToString() + "&" + (100 - course.totalTaskScore));
                }
            }
            return coursesWithNote;
        }

        public void AddTask(string courseName, string taskName, int taskScore)
        {
            Course course = DataSystem.Instance.GetCourse(new Course() { Name = courseName });
            Task taskToAdd = new Task() { TaskName = taskName, MaxScore = taskScore };
            course.AddTask(taskToAdd);
        }

        public List<string> getCoursesWithTasks()
        {
            List<string> coursesWithTask = new List<string>();

            foreach (Course course in DataSystem.Instance.Courses)
            {
                if (course.Tasks.Count > 0)
                {
                    string courseWithTasks = course.ToString() + "&";
                    foreach (Domain.Task task in course.Tasks)
                    {
                        courseWithTasks += task.ToString() + ",";
                    }
                    courseWithTasks = courseWithTasks.Remove(courseWithTasks.Count() - 1);
                    coursesWithTask.Add(courseWithTasks);
                }
            }
            return coursesWithTask;
        }

        public List<string> getCoursesWithTasksToCorrect()
        {
            List<string> coursesWithTasksToCorrect = new List<string>();

            foreach (Course course in DataSystem.Instance.Courses)
            {
                if (course.StudentTasks.Where(x => x.Item2.Item2.Item2 == 0).Count() > 0)
                {
                    string courseWithTasks = course.ToString() + "&";

                    foreach (Domain.Task task in course.Tasks)
                    {
                        if (course.StudentTasks.Where(x => x.Item1.TaskName.Equals(task.TaskName) && x.Item2.Item2.Item2 == 0).Count() > 0)
                        {
                            courseWithTasks += task.ToString() + ",";
                        }
                    }
                    courseWithTasks = courseWithTasks.Remove(courseWithTasks.Count() - 1);
                    coursesWithTasksToCorrect.Add(courseWithTasks);
                }
            }
            return coursesWithTasksToCorrect;
        }

        public List<string> getTasksToCorrect(string course)
        {
            List<string> tasksAtString = new List<string>();
            var tasks = DataSystem.Instance.Courses.Find(x => x.Name.Equals(course)).StudentTasks.Where(x => x.Item2.Item2.Item2 == 0).Select(x => x.Item1);
            foreach (Domain.Task task in tasks)
            {
                if (!tasksAtString.Contains(task.ToString()))
                {
                    tasksAtString.Add(task.ToString());
                }
            }
            return tasksAtString;
        }

        public List<string> getStudentsToCorrect(string course, string task)
        {
            List<string> studentsToCorrect = new List<string>();
            var students = DataSystem.Instance.Courses.Find(x => x.Name.Equals(course)).StudentTasks.Where(x => x.Item1.TaskName.Equals(task)).Select(x => x.Item2);
            foreach (var student in students)
            {
                studentsToCorrect.Add(student.Item1.ToString());
            }
            return studentsToCorrect;
        }

        public void scoreStudent(string courseName, string taskName, int studentNumber, int score)
        {
            Course course = DataSystem.Instance.GetCourse(new Course() { Name = courseName});
            Student student = DataSystem.Instance.GetStudent(new Student() { Number = studentNumber , User = new User()});
            course.RemoveStudentTask(taskName, studentNumber);
            course.AddScoreToTask(taskName, studentNumber, score);
            string newNotifications = "";
            if (DataSystem.Instance.Notifications.Where(x => x.Item1.Number == studentNumber).Count() > 0)
            {
                newNotifications = DataSystem.Instance.Notifications.Find(x => x.Item1.Number == studentNumber).Item2;
                newNotifications += ";Notificación -> En el curso " + courseName + ", en la tarea " + taskName + ", tu calificación es de " + score + " puntos.";
            }
            else
            {
                newNotifications += "Notificación -> En el curso " + courseName + ", en la tarea " + taskName + ", tu calificación es de " + score + " puntos.";
            }
            DataSystem.Instance.CreateNotification(student, newNotifications);
        }
    }
}
