using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class DataSystem
    {
        private static DataSystem instance = null;
        private static readonly object padlock = new object();

        public List<Admin> Admins { get; }
        public List<Student> Students { get; }
        public List<Tuple<Student, string>> Notifications { get; }
        public List<Course> Courses { get; }

        private static readonly object adminsLock= new object();
        private static readonly object studentslock = new object();
        private static readonly object courseslock = new object();
        private static readonly object notificationslock = new object();
        
        private DataSystem()
        {
            this.Students = new List<Student>();
            this.Courses = new List<Course>();
            this.Notifications = new List<Tuple<Student, string>>();
            this.Admins = new List<Admin>();
            this.Admins.Add(new Admin());
        }

        public static DataSystem Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DataSystem();
                    }
                    return instance;
                }
            }
        }

        public bool CheckAdminPassword(Admin adminToLogin)
        {
            lock (adminsLock)
            {
                Admin currentAdmin = this.Admins.Find(x => x.Equals(adminToLogin));
                return (currentAdmin.User.Password == adminToLogin.User.Password);
            }
        }

        public bool AddStudent(Student studentToAdd)
        {
            
            lock (studentslock)
            {
                if (!this.Students.Contains(studentToAdd))
                {
                    studentToAdd.Number = Student.LastStudentRegistered++;
                    this.Students.Add(studentToAdd);
                    return true;
                }
            }
            return false;
        }


        public Student GetStudent(Student studentToFind)
        {
            lock (studentslock)
            {
                return this.Students.Find(x => x.Equals(studentToFind));
            }
        }

        public Course GetCourse(Course courseToFind)
        {
            lock (courseslock)
            {
                return this.Courses.Find(x => x.Equals(courseToFind));
            }
        }

        public void AddCourse(Course courseToAdd)
        {
            lock (courseslock)
            {
                this.Courses.Add(courseToAdd);
            }
        }

        public void RemoveCourse(Course courseToRemove)
        {
            lock (courseslock)
            {
                this.Courses.Remove(courseToRemove);
            }
        }

    }
}
