using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
using Domain;
using System.IO;
using System.Threading.Tasks;
using Communication.Protocol;
using Communication.TcpSockets;

namespace RouteController
{
    public enum Action {
        Login,
        GetCourses,
        GetNotSuscribedCourses,
        Response,
        Suscribe,
        GetSuscribedCourses,
        Unsuscribe,
        GetSuscribedCoursesWithTasks,
        GetCourseTasks,
        UpdateTaskToCourse,
        GetNotifications,
        Logout,
        GetCalifications
    }

    public class ActionDispatcher
    {
        private static Dictionary<Action, string> actionDispatcher = new Dictionary<Action, string>() {
            {Action.Login, "Login"},
            {Action.Logout, "Logout"},
            {Action.GetCourses, "GetCourses"},
            {Action.GetCalifications, "GetCalifications"},
            {Action.GetNotSuscribedCourses, "GetNotSuscribedCourses"},
            {Action.GetSuscribedCourses, "GetSuscribedCourses"},
            {Action.GetSuscribedCoursesWithTasks, "GetSuscribedCoursesWithTasks"},
            {Action.GetCourseTasks, "GetCourseTasks"},
            {Action.GetNotifications, "GetNotifications"},
            {Action.Suscribe, "Suscribe"},
            {Action.Unsuscribe, "Unsuscribe"},
            {Action.UpdateTaskToCourse, "UpdateTaskToCourse"}
        };

        public ActionDispatcher()
        {
        }

        public async Task dispatch(Action action, string data, TcpClient tcpClient)
        {
            string actionString;
            var tcpWriter = new WriteTcpSockets(tcpClient);
            if (actionDispatcher.TryGetValue(action, out actionString))
            {
                Type type = typeof(ActionDispatcher);
                MethodInfo method = type.GetMethod(actionString);
                method.Invoke(this, new object[] { data, tcpWriter });
            }
            else
            {
                throw new Exception("Action not defined.");
            }
        }

        private async Task sendData(Action action, string payload, WriteTcpSockets networkStream)
        {
            var actionInBit = BitConverter.GetBytes((int)action);
            var messageInBytes = Encoding.UTF8.GetBytes(payload);
            var lengthOfDataInBytes = BitConverter.GetBytes(messageInBytes.Length);

            await networkStream.WriteDataAsync(actionInBit);
            await networkStream.WriteDataAsync(lengthOfDataInBytes);
            await networkStream.WriteDataAsync(messageInBytes);
        }

        public async Task Login(string data, WriteTcpSockets networkStream)
        {
            var dataArray = data.Split('&');
            int studentNumber;
            string studentPw = dataArray[1];
            try
            {
                studentNumber = Int32.Parse(dataArray[0]);
            }
            catch
            {
                string studentEmail = dataArray[0];
                studentNumber = GetStudentNumberByEmail(studentEmail);
            }

            if (LoginUser(studentNumber, studentPw))
            {
                Student studentLogged = new Student() { Number = studentNumber, User = new User() };
                if (!IsAlreadyLogged(studentLogged))
                {
                    AddStudentLogged(studentLogged);
                    await sendData(Action.Response, "T&" + studentNumber, networkStream);
                }else
                {
                    await sendData(Action.Response, "L", networkStream);
                }
            }
            else
            {
                await sendData(Action.Response, "F", networkStream);
            }

        }

        private void AddStudentLogged(Student student)
        {
            DataSystem.Instance.AddStudentLogged(DataSystem.Instance.GetStudent(student));
        }

        private bool IsAlreadyLogged(Student student)
        {
            return DataSystem.Instance.IsStudentLogged(student);
        }

        private int GetStudentNumberByEmail(string studentEmail)
        {
            try
            {
                return DataSystem.Instance.Students.Find(x => x.User.Email.Equals(studentEmail)).Number;
            }
            catch
            {
                return 0;
            }
        }

        public bool LoginUser(int studentNumber, string pass)
        {
            User user = new User() { Password = pass }; ;
            Student studentLog = new Student() { User = user, Number = studentNumber };

            try {
                Student currentStudent = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
                return (currentStudent.User.Password.Equals(pass));
            }catch
            {
                return false;
            }
        }

        public void Suscribe(string data, WriteTcpSockets networkStream)
        {
            var dataArray = data.Split('&');
            Course course = DataSystem.Instance.Courses.Find(x => x.Name.Equals(dataArray[0]));
            Student student = new Student() { Number = Int32.Parse(dataArray[1]) , User = new User()};
            if (!course.Students.Select(x=>x.Item1).Contains(student))
            {
                Student studentSub = DataSystem.Instance.Students.Find(x => x.Equals(student));
                course.Students.Add(new Tuple<Student, int>(studentSub, 0));
            }
        }

        public void Unsuscribe(string data, WriteTcpSockets networkStream)
        {
            var dataArray = data.Split('&');
            Course course = DataSystem.Instance.Courses.Find(x => x.Name.Equals(dataArray[0]));
            Student student = new Student() { Number = Int32.Parse(dataArray[1]) , User = new User()};
            if (course.Students.Select(x => x.Item1).Contains(student))
            {
                Student studentUnsub = DataSystem.Instance.Students.Find(x => x.Equals(student));
                course.Students.Remove(course.Students.Find(x => x.Item1.Equals(studentUnsub)));
            }
        }

        public async Task GetCourses(string data, WriteTcpSockets networkStream) {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Select(x=>x.GetList(student)));
            await sendData(Action.Response, coursesString, networkStream);
        }

        public async Task GetNotSuscribedCourses(string data, WriteTcpSockets networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Where(x => !x.Students.Select(y=>y.Item1).Contains(student)).Select(x=>x.ToString()));
            await sendData(Action.Response, coursesString, networkStream);
        }

        public async Task GetSuscribedCourses(string data, WriteTcpSockets networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Where(x => x.Students.Select(y => y.Item1).Contains(student)).Select(x => x.ToString()));
            await sendData(Action.Response, coursesString, networkStream);
        }

        public async Task GetSuscribedCoursesWithTasks(string data, WriteTcpSockets networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Where(x => x.Students.Select(y => y.Item1).Contains(student) && x.Tasks.Count > 0).Select(x => x.ToString()));
            await sendData(Action.Response, coursesString, networkStream);
        }

        public async Task GetCourseTasks(string data, WriteTcpSockets networkStream)
        {
            string courseName = data;
            Course course = DataSystem.Instance.Courses.Find(x => x.Name == courseName);
            string tasksString = string.Join(",", course.Tasks.Select(x => x.ToString()));
            await sendData(Action.Response, tasksString, networkStream);
        }

        public async Task GetNotifications(string data, WriteTcpSockets networkStream)
        {
            string studentNotifications = "";
            if (DataSystem.Instance.Notifications.Where(x => x.Item1.Number.ToString().Equals(data)).Count() > 0)
            {
                studentNotifications = DataSystem.Instance.Notifications.Find(x => x.Item1.Number.ToString().Equals(data)).Item2;
                DataSystem.Instance.Notifications = new List<Tuple<Student,string>>(DataSystem.Instance.Notifications.Where(x => !x.Item1.Number.ToString().Equals(data)));
            }
            await sendData(Action.Response, studentNotifications, networkStream);
        }

        public async Task UpdateTaskToCourse(string data, WriteTcpSockets networkStream)
        {
            int totalToRemove = 0;
            string courseName = data.Split('&')[0];
            totalToRemove += courseName.Length + 1;
            string taskName = data.Split('&')[1];
            totalToRemove += taskName.Length + 1;
            taskName = taskName.Split('[')[0];
            taskName = taskName.Remove(taskName.Count() - 1);
            int studentNumber = Int32.Parse(data.Split('&')[2]);
            totalToRemove += data.Split('&')[2].Length + 1;
            string extension = data.Split('&')[3];
            totalToRemove += extension.Length + 1;
            string fileTask = data.Remove(0, totalToRemove);
            byte[] fileTaskInBytes = Convert.FromBase64String(fileTask);

            string taskPath = "./"+ courseName + "-" + studentNumber + "-" + taskName + extension;

            try
            {
                using (FileStream fs = new FileStream(taskPath, FileMode.Create))
                {
                    if (fs.CanWrite)
                    {
                        await fs.WriteAsync(fileTaskInBytes, 0, fileTaskInBytes.Length).ConfigureAwait(false);
                    }
                }
                Course course = DataSystem.Instance.Courses.Find(x => x.Name.Equals(courseName));
                Student student = course.Students.Find(x => x.Item1.Number == studentNumber).Item1;
                Domain.StudentTask task = course.Tasks.Find(x => x.TaskName.Equals(taskName));
                Tuple<string, int> pathScore = new Tuple<string, int>(taskPath, 0);
                Tuple<Student, Tuple<string, int>> studentPathScore = new Tuple<Student, Tuple<string, int>>(student, pathScore);
                Tuple<Domain.StudentTask, Tuple<Student, Tuple<string, int>>> taskStutendPathScore = new Tuple<Domain.StudentTask, Tuple<Student, Tuple<string, int>>>(task, studentPathScore);
                course.StudentTasks.Add(taskStutendPathScore);
                await sendData(Action.Response, "T", networkStream);
            }
            catch
            {
                await sendData(Action.Response, "F", networkStream);
            }
            
        }

        public async Task  GetCalifications(string data, WriteTcpSockets networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.GetStudent(new Student() { Number = studentNumber , User = new User()});
            List<Course> studentCourses = DataSystem.Instance.GetStudentCourses(student);
            string califications = "";
            foreach (Course course in studentCourses)
            {
                string studentCourseCalification = course.GetStudentCalification(student);
                califications += course.ToString() + " - Calificación: " + studentCourseCalification + "/100&";
                List<Domain.StudentTask> tasks = course.GetTasks();
                foreach(Domain.StudentTask task in tasks)
                {
                    string studentTaskCalification = course.GetStudentTaskCalification(task,student);
                    califications += task.ToString() + " - Calificación: " + studentTaskCalification + ";";
                }
                if(tasks.Count() > 0)
                {
                    califications = califications.Remove(califications.Count() - 1);
                }
                califications += "$";
            }
            if (studentCourses.Count() > 0)
            {
                califications = califications.Remove(califications.Count() - 1);
            }
            await sendData(Action.Response, califications, networkStream);
        }

        public void Logout(string data, WriteTcpSockets networkStream)
        {
            var studentNumber = Int32.Parse(data);
            DataSystem.Instance.RemoveStudentLogged(new Student() { Number = studentNumber, User = new User()});
        }
    }
}
