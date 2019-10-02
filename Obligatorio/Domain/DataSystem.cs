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
        public List<Student> StudentsLogged { get; }
        public List<Tuple<Student, string>> Notifications { get; set; }
        public List<Course> Courses { get; }

        private static readonly object adminsLock= new object();
        private static readonly object studentslock = new object();
        private static readonly object courseslock = new object();
        private static readonly object notificationslock = new object();
        private static readonly object studentloggedlock = new object();

        private DataSystem()
        {
            this.Students = new List<Student>();
            this.StudentsLogged = new List<Student>();
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
                try
                {
                    Admin currentAdmin = this.Admins. Find(x =>x.Equals(adminToLogin));
                    return currentAdmin.User.Password == adminToLogin.User.Password;
                }catch
                {
                    return false;
                }
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

        public bool AddStudentLogged(Student studentToAdd)
        {

            lock (studentloggedlock)
            {
                if (!this.StudentsLogged.Contains(studentToAdd))
                {
                    this.StudentsLogged.Add(studentToAdd);
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

        public bool AddCourse(Course courseToAdd)
        {
            lock (courseslock)
            {
                if (!DataSystem.Instance.Courses.Contains(courseToAdd))
                {
                    this.Courses.Add(courseToAdd);
                    return true;
                }else
                {
                    return false;
                }
                
            }
        }

        public void RemoveCourse(Course courseToRemove)
        {
            lock (courseslock)
            {
                this.Courses.Remove(courseToRemove);
            }
        }

        public void CreateNotification(Student student, string newNotifications)
        {
            lock (notificationslock)
            {
                this.Notifications = new List<Tuple<Student, string>>(DataSystem.Instance.Notifications.Where(x => !x.Item1.Equals(student)));
                this.Notifications.Add(new Tuple<Student, string>(student, newNotifications));
            }
        }

        public bool IsStudentLogged(Student student)
        {
            lock (studentloggedlock)
            {
                if (!this.StudentsLogged.Contains(student))
                {
                    return false;
                }
            }
            return true;
        }

        public void RemoveStudentLogged(Student student)
        {
            lock (studentloggedlock)
            {
                try { 
                    this.StudentsLogged.Remove(student);
                }catch { }
            }
        }

        public List<Course> GetStudentCourses(Student student)
        {
            List<Course> studentCourses = new List<Course>();
            lock (courseslock)
            {
                foreach (Course course in this.Courses)
                {
                    if (course.Students.Select(x => x.Item1).Contains(student))
                    {
                        studentCourses.Add(course);
                    }
                }
            }
            return studentCourses;
        }
    }
}
