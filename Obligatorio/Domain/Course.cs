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
        public List<Task> Tasks = new List<Task>();
        public List<Tuple<Student,Tuple<Task,Tuple<string, int>>>> StudentTasks = new List<Tuple<Student, Tuple<Task, Tuple<string, int>>>>();
        public int totalTaskScore = 0;

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
    }
}
