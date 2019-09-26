using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User
    {
        public string UserNumber { get; set; }
        public string Password { get; set; }

        public override bool Equals(object obj)
        {
            return this.UserNumber.Equals(((User)obj).UserNumber);
        }

    }
}
