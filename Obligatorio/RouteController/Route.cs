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
        Login
    }

    public class ActionDispatcher
    {
        private static Dictionary<Action, string> actionDispatcher = new Dictionary<Action, string>() {
            {Action.Login, "Login"}
        };
        private List<Admin> admins;
        private List<Student> students;
        static readonly object _locker = new object();

        public ActionDispatcher()
        {
            this.students = new List<Student>();
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

        public void Login(string data, NetworkStream networkStream)
        {
            Dictionary<string, string> dataDictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(data);
            if(LoginUser(dataDictionary["Username"], dataDictionary["Password"]))
            {
                var response = BitConverter.GetBytes(1);
                networkStream.Write(response, 0, response.Length);
            }
            else
            {
                var response = BitConverter.GetBytes(0);
                networkStream.Write(response, 0, response.Length);
            }

        }

        public bool LoginUser(string username, string pass)
        {
            User user = new User() { Username = username, Password = pass };
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

        public bool LoginAdmin(string username, string pass)
        {
            User user = new User() { Username = username, Password = pass };
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
            User studentUser = new User() { Username = studentUsername, Password = studentPass };

            lock (_locker)
            {
                this.students.Add(new Student() { User = studentUser });
                
            }
        }
    }

    
}
