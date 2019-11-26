using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class DataSystem
    {
        private static DataSystem instance = null;
        private static Semaphore padlock = new Semaphore(1, 1);

        private string queuePath;
        private MessageQueue messageQueue;

        public List<Admin> Admins { get; }
        public List<Student> Students { get; }
        public List<Student> StudentsLogged { get; }
        public List<Tuple<Student, string>> Notifications { get; set; }
        public List<Course> Courses { get; }
        public List<Teacher> Teachers { get; }
        public List<Guid> TeachersLoggeds { get; }

        private Semaphore adminsLock = new Semaphore(1, 1);
        private Semaphore studentslock = new Semaphore(1, 1);
        private Semaphore courseslock = new Semaphore(1, 1);
        private Semaphore notificationslock = new Semaphore(1, 1);
        private Semaphore studentloggedlock = new Semaphore(1, 1);
        private Semaphore teacherslock = new Semaphore(1, 1);
        private Semaphore logslock = new Semaphore(1, 1);
        private Semaphore teachersloggedslock = new Semaphore(1, 1);

        private DataSystem()
        {
            this.Students = new List<Student>();
            this.StudentsLogged = new List<Student>();
            this.Courses = new List<Course>();
            this.Notifications = new List<Tuple<Student, string>>();
            this.Admins = new List<Admin>();
            this.Teachers = new List<Teacher>();
            this.TeachersLoggeds = new List<Guid>();
            this.Admins.Add(new Admin());
        }

        private void WriteInQueue(Log log)
        {
            using (var messageQueue = new MessageQueue(this.queuePath))
            {
                messageQueue.Send(log);
            }
        }

        public void SetQuewePath(string queuePath)
        {
            this.queuePath = queuePath;
            this.messageQueue = new MessageQueue(queuePath)
            {
                Formatter = new XmlMessageFormatter(new[] { typeof(Log) })
            };
        }

        public static DataSystem Instance
        {
            get
            {
                padlock.WaitOne();
                    if (instance == null)
                    {
                        instance = new DataSystem();
                    }
                padlock.Release();
                    return instance;
            }
        }

        public Guid CheckAdminPassword(Admin adminToLogin)
        {
            adminsLock.WaitOne();
                try
                {
                    Admin currentAdmin = this.Admins. Find(x =>x.Equals(adminToLogin));
                    if (currentAdmin.User.Password == adminToLogin.User.Password)
                    {
                        teachersloggedslock.WaitOne();
                        Guid token = Guid.NewGuid();
                        this.TeachersLoggeds.Add(token);
                        teachersloggedslock.Release();
                        adminsLock.Release();
                        return token;
                    }
                    else
                    {
                        adminsLock.Release();
                        return Guid.Empty;
                    }
                }catch
                {
                    adminsLock.Release();
                    return Guid.Empty;
            }
        }

        public Guid CheckTeacherPassword(Teacher teacherToLogin)
        {

            teacherslock.WaitOne();
            try{
                    Teacher currentTeacher = this.Teachers.Find(x => x.Equals(teacherToLogin));
                    if (currentTeacher.User.Password == teacherToLogin.User.Password)
                    {
                        teachersloggedslock.WaitOne();
                        Guid token = Guid.NewGuid();
                        this.TeachersLoggeds.Add(token);
                        teachersloggedslock.Release();
                        teacherslock.Release();
                        return token;
                    }
                    else
                    {
                        teacherslock.Release();
                        throw new ArgumentException("El correo electrónico o contraseña es incorrecto.");
                    }
                }
                catch
                {
                    teacherslock.Release();
                    throw new ArgumentException("No existe un docente con ese correo electrónico.");
                }
        }

        public bool AddStudent(Student studentToAdd)
        {

             studentslock.WaitOne();
                if (!this.Students.Contains(studentToAdd))
                {
                    studentToAdd.Number = Student.LastStudentRegistered++;
                    this.Students.Add(studentToAdd);
                    logslock.WaitOne();
                    WriteInQueue(new Log(DateTime.Now, "Se ha creado al estudiante: " + studentToAdd.Number + " (" + studentToAdd.User.Email + ").", 0));
                    logslock.Release();
                    studentslock.Release(); 
                    return true;
                }
            studentslock.Release();
            return false;
        }


        public bool AddStudentLogged(Student studentToAdd)
        {

            studentloggedlock.WaitOne();
                if (!this.StudentsLogged.Contains(studentToAdd))
                {
                    this.StudentsLogged.Add(studentToAdd);
                    studentloggedlock.Release();
                    return true;
                }
            studentloggedlock.Release();
            return false;
        }

        public Student GetStudent(Student studentToFind)
        {
            studentslock.WaitOne();
            Student student = this.Students.Find(x => x.Equals(studentToFind));
            studentslock.Release();
            return student;
        }

        public Teacher AddTeacher(Teacher teacher)
        {

            teacherslock.WaitOne();
                if (!this.Teachers.Contains(teacher))
                {
                    this.Teachers.Add(teacher);
                    logslock.WaitOne();
                        WriteInQueue(new Log(DateTime.Now, "Se ha creado al docente: " + teacher.Name + " " + teacher.Surname + " (" + teacher.User.Email + ").", 1));
                    logslock.Release();
                    teacherslock.Release();
                    return teacher;
                }
                else
                {
                    teacherslock.Release();
                    throw new ArgumentException("Ya existe un profesor con ese correo electrónico.");
                }
        }

        public Teacher GetTeacher(Teacher teacher)
        {
            teacherslock.WaitOne();
                if (this.Teachers.Exists(x => x.Equals(teacher)))
                {
                    teacherslock.Release();
                    return this.Teachers.Find(x => x.Equals(teacher));
                }
                else
                {
                    teacherslock.Release();
                    throw new ArgumentException("No existe un profesor con ese correo electrónico.");
                }
        }

        public Course GetCourse(Course courseToFind)
        {
            courseslock.WaitOne();
            Course course = this.Courses.Find(x => x.Equals(courseToFind));
            courseslock.Release();
            return course;
        }

        public bool AddCourse(Course courseToAdd)
        {
            courseslock.WaitOne();
                if (!DataSystem.Instance.Courses.Contains(courseToAdd))
                {

                    this.Courses.Add(courseToAdd);
                    logslock.WaitOne();
                        WriteInQueue(new Log(DateTime.Now, "Se ha creado el curso: " + courseToAdd.Name, 2));
                    logslock.Release();
                    courseslock.Release();
                    return true;
                }
                else
                {
                    courseslock.Release();
                    return false;
                }
        }

        public void Suscribe(Student studentSub, Course course)
        {
            logslock.WaitOne();
            WriteInQueue(new Log(DateTime.Now, "Es estudiante " + studentSub.Number + " se ha inscripto al curso " + course.Name + ".", 4));
            logslock.Release();
        }

        public void RemoveCourse(Course courseToRemove)
        {
            courseslock.WaitOne();
                this.Courses.Remove(courseToRemove);
                logslock.WaitOne();
                WriteInQueue(new Log(DateTime.Now, "Se ha removido el curso: " + courseToRemove.Name, 3));
                logslock.Release();
            courseslock.Release();
        }

        public bool ExistToken(Guid token)
        {
            return this.TeachersLoggeds.Contains(token);
        }

        public void CreateNotification(Student student, string newNotifications)
        {
            notificationslock.WaitOne();
                this.Notifications = new List<Tuple<Student, string>>(DataSystem.Instance.Notifications.Where(x => !x.Item1.Equals(student)));
                this.Notifications.Add(new Tuple<Student, string>(student, newNotifications));
            notificationslock.Release();
        }

        public bool IsStudentLogged(Student student)
        {
            studentloggedlock.WaitOne();
                if (!this.StudentsLogged.Contains(student))
                {
                    studentloggedlock.Release();
                    return false;
                }
            studentloggedlock.Release();
            return true;
        }

        public void RemoveStudentLogged(Student student)
        {
            studentloggedlock.WaitOne();
            try { 
                this.StudentsLogged.Remove(student);
            studentloggedlock.Release();
            }
            catch
            {
                studentloggedlock.Release();
            }
        }

        public List<Course> GetStudentCourses(Student student)
        {
            List<Course> studentCourses = new List<Course>();
            courseslock.WaitOne();
                foreach (Course course in this.Courses)
                {
                    if (course.Students.Select(x => x.Item1).Contains(student))
                    {
                        studentCourses.Add(course);
                    }
                }
            courseslock.Release();
            return studentCourses;
        }

        public void CorrectTask(Student student, string courseName, string taskName, int score)
        {
            logslock.WaitOne();
            WriteInQueue(new Log(DateTime.Now, "Se ha calificado al estudiante " + student.Number + " en la tarea "+taskName+" del curso "+courseName+", su calificación fué: "+ score +".", 5));
            logslock.Release();
        }

        public List<Log> GetLogs()
        {
            var logs = messageQueue.GetAllMessages();
            List<Log> logsList = new List<Log>();
            foreach (var log in logs)
            {
                logsList.Add((Log)log.Body);
            }
            return logsList;
        }

        public List<Log> GetLogsByType(int typeLog)
        {
            List<Log> logsList = GetLogs();
            return logsList.FindAll(l => l.Type == (LogType)typeLog);
        }
    }
}
