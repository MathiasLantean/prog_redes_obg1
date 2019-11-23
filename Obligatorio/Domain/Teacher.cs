using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Teacher
    {
        public User User { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public override string ToString()
        {
            return Name + " " + Surname + " (" + User.Email +")";  
        }

        public override bool Equals(object obj)
        {
            return this.User.Equals(((Teacher)obj).User);
        }
    }
}
