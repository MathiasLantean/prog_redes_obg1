using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;
using Domain;
using System.Web.Script.Serialization;
using System.Threading;
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
        UpdateTaskToCourse
    }

    public class ActionDispatcher
    {
        private static Dictionary<Action, string> actionDispatcher = new Dictionary<Action, string>() {
            {Action.Login, "Login"},
            {Action.GetCourses, "GetCourses"},
            {Action.GetNotSuscribedCourses, "GetNotSuscribedCourses"},
            {Action.GetSuscribedCourses, "GetSuscribedCourses"},
            {Action.GetSuscribedCoursesWithTasks, "GetSuscribedCoursesWithTasks"},
            {Action.GetCourseTasks, "GetCourseTasks"},
            {Action.Suscribe, "Suscribe"},
            {Action.Unsuscribe, "Unsuscribe"},
            {Action.UpdateTaskToCourse, "UpdateTaskToCourse"}
        };
        private List<Admin> admins;
        private List<Student> students;
        private List<Course> courses;
        private int lastStudentRegistered = 200000;
        static readonly object _locker = new object();
        static readonly object _lockerCourse = new object();

        public ActionDispatcher()
        {
            this.students = new List<Student>();
            this.courses = new List<Course>();
            this.admins = new List<Admin>();
            this.admins.Add(new Admin()); 
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

        public bool LoginUser(int studentNumber, string pass)
        {
            User user;
            Student studentLog;
            
            user = new User() { Password = pass };
            studentLog = new Student() { User = user, Number = studentNumber };

            lock(_locker){
                if (this.students.Contains(studentLog))
                {
                    Student currentStudent = this.students.Find(x => x.Equals(studentLog));
                    return (currentStudent.User.Password == pass);
                }
            }
            return false;
        }

        public void addCourse(string newCourse)
        {
            Course course = new Course() { Name = newCourse };

            lock (_lockerCourse)
            {
                this.courses.Add(course);
            }
        }

        public void removeCourse(int removeCourse)
        {
            lock (_lockerCourse)
            {
                this.courses.RemoveAt(removeCourse);
            }
        }

        public List<string> getCousesAtString()
        {
            List<string> coursesAtString = new List<string>();
            foreach(Course course in courses)
            {
                coursesAtString.Add(course.ToString());
            }
            return coursesAtString;
        }

        public List<string> getCoursesWithTasks()
        {
            List<string> coursesWithTask = new List<string>();

            foreach (Course course in courses)
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

        public void AddTask(string courseName, string taskName, int taskScore)
        {
            Course course = this.courses.Find(x => x.Name.Equals(courseName));
            Domain.Task taskToAdd = new Domain.Task() { TaskName = taskName, MaxScore = taskScore };
            course.Tasks.Add(taskToAdd);
            course.totalTaskScore = course.totalTaskScore + taskScore;
        }

        public List<string> getPosibleCoursesToAddTask()
        {
            List<string> coursesWithNote = new List<string>();
            foreach (Course course in courses)
            {
                if(course.totalTaskScore < 100)
                {
                    coursesWithNote.Add(course.ToString() + "&" + (100 - course.totalTaskScore));
                }
            }
            return coursesWithNote;
        }

        public bool LoginAdmin(string username, string pass)
        {
            User user = new User() { Email = username, Password = pass };
            Admin adminLog = new Admin() { User = user };

            lock (_locker)
            {
                if (this.admins.Contains(adminLog))
                {
                    Admin currentAdmin = this.admins.Find(x => x.Equals(adminLog));
                    return (currentAdmin.User.Password == pass);
                }

            }
            return false;
        }

        public bool addStudent(string studentUsername, string studentPass)
        {
            User studentUser = new User() { Email = studentUsername, Password = studentPass };

            lock (_locker)
            {
                if (!this.students.Contains(new Student() { User = studentUser }))
                {
                    this.students.Add(new Student() { User = studentUser, Number = this.lastStudentRegistered++ });
                    return true;
                }
            }
            return false;
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
                sendData(Action.Response, "T&" + studentNumber, networkStream);
            }
            else
            {
                sendData(Action.Response, "F", networkStream);
            }

        }

        private int GetStudentNumberByEmail(string studentEmail)
        {
            try
            {
                return this.students.Find(x => x.User.Email.Equals(studentEmail)).Number;
            }
            catch
            {
                return 0;
            }
        }

        public void Suscribe(string data, NetworkStream networkStream)
        {
            var dataArray = data.Split('&');
            Course course = this.courses.Find(x => x.Name.Equals(dataArray[0]));
            Student student = new Student() { Number = Int32.Parse(dataArray[1]) };
            if (!course.Students.Select(x=>x.Item1).Contains(student))
            {
                Student studentSub = this.students.Find(x => x.Equals(student));
                course.Students.Add(new Tuple<Student, int>(studentSub, 0));
            }
        }

        public void Unsuscribe(string data, NetworkStream networkStream)
        {
            var dataArray = data.Split('&');
            Course course = this.courses.Find(x => x.Name.Equals(dataArray[0]));
            Student student = new Student() { Number = Int32.Parse(dataArray[1]) };
            if (course.Students.Select(x => x.Item1).Contains(student))
            {
                Student studentUnsub = this.students.Find(x => x.Equals(student));
                course.Students.Remove(course.Students.Find(x => x.Item1.Equals(studentUnsub)));
            }
        }

        public void GetCourses(string data, NetworkStream networkStream) {
            int studentNumber = Int32.Parse(data);
            Student student = this.students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", this.courses.Select(x=>x.GetList(student)));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetNotSuscribedCourses(string data, NetworkStream networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = this.students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", this.courses.Where(x => !x.Students.Select(y=>y.Item1).Contains(student)).Select(x=>x.ToString()));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetSuscribedCourses(string data, NetworkStream networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = this.students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", this.courses.Where(x => x.Students.Select(y => y.Item1).Contains(student)).Select(x => x.ToString()));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetSuscribedCoursesWithTasks(string data, NetworkStream networkStream)
        {
            int studentNumber = Int32.Parse(data);
            Student student = this.students.Find(x => x.Number == studentNumber);
            string coursesString = string.Join(",", this.courses.Where(x => x.Students.Select(y => y.Item1).Contains(student) && x.Tasks.Count > 0).Select(x => x.ToString()));
            sendData(Action.Response, coursesString, networkStream);
        }

        public void GetCourseTasks(string data, NetworkStream networkStream)
        {
            string courseName = data;
            Course course = this.courses.Find(x => x.Name == courseName);
            string tasksString = string.Join(",", course.Tasks.Select(x => x.ToString()));
            sendData(Action.Response, tasksString, networkStream);
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

            string taskPath = "./"+ courseName + "-" + studentNumber + "-" + taskName + "-" + extension;

            try
            {
                File.WriteAllBytes(taskPath, fileTaskInBytes);
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
    }

    
}
