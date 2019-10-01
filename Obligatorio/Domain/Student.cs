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

        public static int LastStudentRegistered { get; set; } = 200000;

        public override string ToString()
        {
            return Number + " (" + User.Email + ")";  
        }

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
