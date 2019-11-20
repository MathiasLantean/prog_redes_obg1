using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Course
    {
        public string Name { get; set; }
        public List<Tuple<Student, int>> Students = new List<Tuple<Student, int>>();
        public List<StudentTask> Tasks = new List<StudentTask>();
        public List<Tuple<StudentTask,Tuple<Student, Tuple<string, int>>>> StudentTasks = new List<Tuple<StudentTask, Tuple<Student, Tuple<string, int>>>>();
        public int totalTaskScore = 0;

        private static readonly object studentslock = new object();
        private static readonly object studentTaskslock = new object();
        private static readonly object taskslock = new object();

        public override bool Equals(object obj)
        {
            return this.Name.Equals(((Course)obj).Name);
        }
        public override string ToString()
        {
            return this.Name;
        }

        public string GetList(Student student)
        {

            if (Students.Select(x=>x.Item1).Contains(student))
            {
                return this.Name + " | Inscripto | Calificación: " + Students.Find(x => x.Item1.Equals(student)).Item2;
            }else
            {
                return this.Name;
            }
        }

        public void RemoveStudentTask(string taskName, int studentNumber)
        {
            lock (studentTaskslock)
            {
                this.StudentTasks = new List<Tuple<StudentTask, Tuple<Student, Tuple<string, int>>>>(StudentTasks.Where(x => !(x.Item1.TaskName.Equals(taskName) && x.Item2.Item1.Number == studentNumber)));
            }
        }

        public void AddTask(StudentTask taskToAdd)
        {
            lock (taskslock)
            {
                this.Tasks.Add(taskToAdd);
                this.totalTaskScore += taskToAdd.MaxScore;
            }
        }
        public void AddScoreToTask(string taskName, int studentNumber, int score)
        {
            StudentTask task = this.Tasks.Find(x => x.TaskName.Equals(taskName));
            Student student = this.Students.Find(x=>x.Item1.Number == studentNumber).Item1;

            Tuple<string, int> corrected = new Tuple<string, int>("Corregido", score);
            Tuple<Student, Tuple<string, int>> studentCorrected = new Tuple<Student, Tuple<string, int>>(student, corrected);
            lock (studentTaskslock)
            {
                this.StudentTasks.Add(new Tuple<StudentTask, Tuple<Student, Tuple<string, int>>>(task, studentCorrected));
            }

            CorrectTotalScore(studentNumber, score);
        }

        private void CorrectTotalScore(int studentNumber, int score)
        {
            int oldScore = this.Students.Find(x => x.Item1.Number == studentNumber).Item2;
            Student student = this.Students.Find(x => x.Item1.Number == studentNumber).Item1;
            int newScore = oldScore + score;
            Tuple<Student, int> studentScore = new Tuple<Student, int>(student, score);
            lock (studentslock)
            {
                this.Students = new List<Tuple<Student, int>>(Students.Where(x => x.Item1.Number != studentNumber));
                this.Students.Add(studentScore);
            }
        }

        public List<StudentTask> GetTasks()
        {
            return this.Tasks;
        }

        public string GetStudentCalification(Student student)
        {
            try
            {
                return this.Students.Find(x => x.Item1.Equals(student)).Item2.ToString();
            }catch
            {
                return "0";
            }
        }

        public string GetStudentTaskCalification(StudentTask task, Student student)
        {
            try
            {
                return this.StudentTasks.Where(x => x.Item1.Equals(task)).Select(x => x.Item2).Where(x => x.Item1.Equals(student)).Select(x => x.Item2).First().Item2.ToString();
            }
            catch
            {
                return "0";
            }
            
        }
    }
}
