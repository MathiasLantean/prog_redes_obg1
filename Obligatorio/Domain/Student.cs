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
        public int Number { get; set; }

        public override bool Equals(object obj)
        {
            if (this.Number == ((Student)obj).Number)
            {
                return true;
            }else
            {
                return this.User.Equals(((Student)obj).User);
            }
        }
    }
}
