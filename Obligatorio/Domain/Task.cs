using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Task
    {
        public Student Student { get; set; }
        public string PathTask { get; set; }

        public override bool Equals(object obj)
        {
            return this.PathTask.Equals(((Task)obj).PathTask) && this.Student.Equals(((Task)obj).Student);
        }
    }
}
