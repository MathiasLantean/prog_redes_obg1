using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class StudentTask
    {
        public string TaskName { get; set; }
        public int MaxScore { get; set; }

        public override bool Equals(object obj)
        {
            return this.TaskName.Equals(((StudentTask)obj).TaskName);
        }
        public override string ToString()
        {
            return this.TaskName + " [Calificación máxima: " + MaxScore + "]";
        }
    }
}
