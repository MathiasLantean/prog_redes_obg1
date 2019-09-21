using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Student
    {
        public User User { get; set; }

        public override bool Equals(object obj)
        {
            return this.User.Equals(((Student)obj).User);
        }
    }
}
