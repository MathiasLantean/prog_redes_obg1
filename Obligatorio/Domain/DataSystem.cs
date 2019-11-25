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
        public List<Teacher> Teachers { get; }
        public List<Log> Logs { get; }
        public List<Guid> TeachersLoggeds { get; }

        private static readonly object adminsLock= new object();
        private static readonly object studentslock = new object();
        private static readonly object courseslock = new object();
        private static readonly object notificationslock = new object();
        private static readonly object studentloggedlock = new object();
        private static readonly object teacherslock = new object();
        private static readonly object logslock = new object();
        private static readonly object teachersloggedslock = new object();

        private DataSystem()
        {
            this.Students = new List<Student>();
            this.StudentsLogged = new List<Student>();
            this.Courses = new List<Course>();
            this.Notifications = new List<Tuple<Student, string>>();
            this.Admins = new List<Admin>();
            this.Teachers = new List<Teacher>();
            this.Logs = new List<Log>();
            this.TeachersLoggeds = new List<Guid>();
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

        public Guid CheckAdminPassword(Admin adminToLogin)
        {
            lock (adminsLock)
            {
                try
                {
                    Admin currentAdmin = this.Admins. Find(x =>x.Equals(adminToLogin));
                    if (currentAdmin.User.Password == adminToLogin.User.Password)
                    {
                        lock (teachersloggedslock)
                        {
                            Guid token = Guid.NewGuid();
                            this.TeachersLoggeds.Add(token);
                            return token;
                        }
                    }
                    else
                    {
                        return Guid.Empty;
                    }
                }catch
                {
                    return Guid.Empty;
                }
            }
        }

        public Guid CheckTeacherPassword(Teacher teacherToLogin)
        {
            lock (teacherslock)
            {
                try
                {
                    Teacher currentTeacher = this.Teachers.Find(x => x.Equals(teacherToLogin));
                    if (currentTeacher.User.Password == teacherToLogin.User.Password)
                    {
                        lock (teachersloggedslock)
                        {
                            Guid token = Guid.NewGuid();
                            this.TeachersLoggeds.Add(token);
                            return token;
                        }
                    }
                    else
                    {
                        throw new ArgumentException("El correo electrónico o contraseña es incorrecto.");
                    }
                }
                catch
                {
                    throw new ArgumentException("No existe un docente con ese correo electrónico.");
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
                    lock (logslock)
                    {
                        this.Logs.Add(new Log(DateTime.Now, "Se ha creado al estudiante: "+ studentToAdd.Number +" ("+ studentToAdd.User.Email + ").", 0));
                        return true;
                    }
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

        public Teacher AddTeacher(Teacher teacher)
        {

            lock (teacherslock)
            {
                if (!this.Teachers.Contains(teacher))
                {
                    this.Teachers.Add(teacher);
                    lock (logslock)
                    {
                        this.Logs.Add(new Log(DateTime.Now, "Se ha creado al docente: " + teacher.Name + " " + teacher.Surname + " (" + teacher.User.Email + ").", 1));
                        return teacher;
                    }
                }
                else
                {
                    throw new ArgumentException("Ya existe un profesor con ese correo electrónico.");
                }
            }
        }

        public Teacher GetTeacher(Teacher teacher)
        {
            lock (teacherslock)
            {
                if (this.Teachers.Exists(x => x.Equals(teacher)))
                {
                    return this.Teachers.Find(x => x.Equals(teacher));
                }
                else
                {
                    throw new ArgumentException("No existe un profesor con ese correo electrónico.");
                }
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
                    lock (logslock)
                    {
                        this.Logs.Add(new Log(DateTime.Now, "Se ha creado el curso: " + courseToAdd.Name, 2));
                        return true;
                    }
                }
                else
                {
                    return false;
                }
                
            }
        }

        public void Suscribe(Student studentSub, Course course)
        {
            lock (logslock)
            {
                this.Logs.Add(new Log(DateTime.Now, "Es estudiante " + studentSub.Number + " se ha inscripto al curso " + course.Name + ".", 4));
            }
        }

        public void RemoveCourse(Course courseToRemove)
        {
            lock (courseslock)
            {
                this.Courses.Remove(courseToRemove);
                lock (logslock)
                {
                    this.Logs.Add(new Log(DateTime.Now, "Se ha removido el curso: " + courseToRemove.Name, 3));
                }
            }
        }

        public bool ExistToken(Guid token)
        {
            return this.TeachersLoggeds.Contains(token);
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

        public void CorrectTask(Student student, string courseName, string taskName, int score)
        {
            lock (logslock)
            {
                this.Logs.Add(new Log(DateTime.Now, "Se ha calificado al estudiante " + student.Number + " en la tarea "+taskName+" del curso "+courseName+", su calificación fué: "+ score +".", 5));
            }
        }

        public List<Log> GetLogsByType(int typeLog)
        {
            return this.Logs.FindAll(l => l.Type == (LogType)typeLog);
        }
    }
}
