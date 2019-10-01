using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
using Domain;
using System.IO;

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

        public void dispatch(Action action, string data, NetworkStream networkStream)
        {
            string actionString;
            if (actionDispatcher.TryGetValue(action, out actionString))
            {
                Type type = typeof(ActionDispatcher);
                MethodInfo method = type.GetMethod(actionString);
                method.Invoke(this, new object[] {data, networkStream });
            }
            else
            {
                throw new Exception("Action not defined.");
            }


        }

        private void sendData(Action action, string payload, NetworkStream networkStream)
        {
            var actionInBit = BitConverter.GetBytes((int)action);
            var messageInBytes = Encoding.UTF8.GetBytes(payload);
            var lengthOfDataInBytes = BitConverter.GetBytes(messageInBytes.Length);

            networkStream.Write(actionInBit, 0, actionInBit.Length);
            networkStream.Write(lengthOfDataInBytes, 0, lengthOfDataInBytes.Length);
            networkStream.Write(messageInBytes, 0, messageInBytes.Length);
        }

        public void Login(string data, NetworkStream networkStream)
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
                Student studentLogged = new Student() { Number = studentNumber };
                if (!IsAlreadyLogged(studentLogged))
                {
                    AddStudentLogged(studentLogged);
                    sendData(Action.Response, "T&" + studentNumber, networkStream);
                }else
                {
                    sendData(Action.Response, "L", networkStream);
                }
            }
            else
            {
                sendData(Action.Response, "F", networkStream);
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

        public void Suscribe(string data, NetworkStream networkStream)
        {
            var dataArray = data.Split('&');
            Course course = DataSystem.Instance.Courses.Find(x => x.Name.Equals(dataArray[0]));
            Student student = new Student() { Number = Int32.Parse(dataArray[1]) };
            if (!course.Students.Select(x=>x.Item1).Contains(student))
            {
                Student studentSub = DataSystem.Instance.Students.Find(x => x.Equals(student));
                course.Students.Add(new Tuple<Student, int>(studentSub, 0));
            }
        }

        public void Unsuscribe(string data, NetworkStream networkStream)
        {
            var dataArray = data.Split('&');
            Course course = DataSystem.Instance.Courses.Find(x => x.Name.Equals(dataArray[0]));
            Student student = new Student() { Number = Int32.Parse(dataArray[1]) };
            if (course.Students.Select(x => x.Item1).Contains(student))
            {
                Student studentUnsub = DataSystem.Instance.Students.Find(x => x.Equals(student));
                course.Students.Remove(course.Students.Find(x => x.Item1.Equals(studentUnsub)));
            }
        }

        public void GetCourses(string data, NetworkStream networkStream) {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Select(x=>x.GetList(student)));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetNotSuscribedCourses(string data, NetworkStream networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Where(x => !x.Students.Select(y=>y.Item1).Contains(student)).Select(x=>x.ToString()));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetSuscribedCourses(string data, NetworkStream networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Where(x => x.Students.Select(y => y.Item1).Contains(student)).Select(x => x.ToString()));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetSuscribedCoursesWithTasks(string data, NetworkStream networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.Students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", DataSystem.Instance.Courses.Where(x => x.Students.Select(y => y.Item1).Contains(student) && x.Tasks.Count > 0).Select(x => x.ToString()));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetCourseTasks(string data, NetworkStream networkStream)
        {
            string courseName = data;
            Course course = DataSystem.Instance.Courses.Find(x => x.Name == courseName);
            string tasksString = string.Join(",", course.Tasks.Select(x => x.ToString()));
            sendData(Action.Response, tasksString, networkStream);
        }

        public void GetNotifications(string data, NetworkStream networkStream)
        {
            string studentNotifications = "";
            if (DataSystem.Instance.Notifications.Where(x => x.Item1.Number.ToString().Equals(data)).Count() > 0)
            {
                studentNotifications = DataSystem.Instance.Notifications.Find(x => x.Item1.Number.ToString().Equals(data)).Item2;
                DataSystem.Instance.Notifications = new List<Tuple<Student,string>>(DataSystem.Instance.Notifications.Where(x => !x.Item1.Number.ToString().Equals(data)));
            }
            sendData(Action.Response, studentNotifications, networkStream);
        }

        public void UpdateTaskToCourse(string data, NetworkStream networkStream)
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
            char[] fileTaskInChar = StringAChar(fileTask);
            byte[] fileTaskInBytes = new byte[fileTaskInChar.Length];
            for (int i = 0; i < fileTaskInChar.Length; i++)
            {
                fileTaskInBytes[i] = (byte)fileTaskInChar[i];
            }

            string taskPath = "./"+ courseName + "-" + studentNumber + "-" + taskName + extension;

            try
            {
                File.WriteAllBytes(taskPath, fileTaskInBytes);
                Course course = DataSystem.Instance.Courses.Find(x => x.Name.Equals(courseName));
                Student student = course.Students.Find(x => x.Item1.Number == studentNumber).Item1;
                Domain.Task task = course.Tasks.Find(x => x.TaskName.Equals(taskName));
                Tuple<string, int> pathScore = new Tuple<string, int>(taskPath, 0);
                Tuple<Student, Tuple<string, int>> studentPathScore = new Tuple<Student, Tuple<string, int>>(student, pathScore);
                Tuple< Domain.Task,Tuple <Student, Tuple<string, int>>> taskStutendPathScore = new Tuple<Domain.Task,Tuple<Student, Tuple<string, int>>>(task, studentPathScore);
                course.StudentTasks.Add(taskStutendPathScore);
                sendData(Action.Response, "T", networkStream);
            }
            catch
            {
                sendData(Action.Response, "F", networkStream);
            }
            
        }
        private char[] StringAChar(string str)
        {
            char[] result = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                result[i] = str[i];
            }
            return result;
        }

        public void GetCalifications(string data, NetworkStream networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = DataSystem.Instance.GetStudent(new Student() { Number = studentNumber });
            List<Course> studentCourses = DataSystem.Instance.GetStudentCourses(student);
            string califications = "";
            foreach (Course course in studentCourses)
            {
                string studentCourseCalification = course.GetStudentCalification(student);
                califications += course.ToString() + " - Calificación: " + studentCourseCalification + "/100&";
                List<Domain.Task> tasks = course.GetTasks();
                foreach(Domain.Task task in tasks)
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
            sendData(Action.Response, califications, networkStream);
        }

        public void Logout(string data, NetworkStream networkStream)
        {
            var studentNumber = Int32.Parse(data);
            DataSystem.Instance.RemoveStudentLogged(new Student() { Number = studentNumber });
        }
    }
}
