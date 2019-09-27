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

namespace RouteController
{
    public enum Action {
        Login,
        GetCourses,
        Response,
        Suscribe
    }

    public class ActionDispatcher
    {
        private static Dictionary<Action, string> actionDispatcher = new Dictionary<Action, string>() {
            {Action.Login, "Login"},
            {Action.GetCourses, "GetCourses"},
            {Action.Suscribe, "Suscribe"}
        };
        private List<Admin> admins;
        private List<Student> students;
        private List<Course> courses;
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

        public bool LoginUser(string username, string pass)
        {
            User user = new User() { UserNumber = username, Password = pass };
            Student studentLog = new Student() { User = user };
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

        public bool LoginAdmin(string username, string pass)
        {
            User user = new User() { UserNumber = username, Password = pass };
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

        public void addStudent(string studentUsername, string studentPass)
        {
            User studentUser = new User() { UserNumber = studentUsername, Password = studentPass };

            lock (_locker)
            {
                this.students.Add(new Student() { User = studentUser });
                
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
            if (LoginUser(dataArray[0], dataArray[1]))
            {
                sendData(Action.Response, "T&" + dataArray[0], networkStream);
            }
            else
            {
                sendData(Action.Response, "F", networkStream);
            }

        }

        public void Suscribe(string data, NetworkStream networkStream)
        {
            var dataArray = data.Split('&');
            Course course = this.courses.Find(x => x.Name.Equals(dataArray[0]));
            Student student = this.students.Find(x => x.User.UserNumber.Equals(dataArray[1]));
            course.Students.Add(student);
        }

        public void GetCourses(string data, NetworkStream networkStream) {
            string coursesString = string.Join(",", this.courses.Select(x=>x.ToString()));
            sendData(Action.Response, coursesString, networkStream);
            
        }
    }

    
}
