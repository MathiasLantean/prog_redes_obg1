using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public User()
        {
            Email = "";
            Password = "";
        }

        public override bool Equals(object obj)
        {
            return this.Email.Equals(((User)obj).Email);
        }

    }
}
