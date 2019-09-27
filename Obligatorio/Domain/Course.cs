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
        public List<Student> Students = new List<Student>();

        public override bool Equals(object obj)
        {
            return this.Name.Equals(((Course)obj).Name);
        }
        public override string ToString()
        {
            return this.Name;
        }

    }
}
