using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Admin
    {
        public User User = new User() { Email = "admin", Password = "admin" };

        public override bool Equals(object obj)
        {
            return this.User.Equals(((Admin)obj).User);
        }
    }
}
