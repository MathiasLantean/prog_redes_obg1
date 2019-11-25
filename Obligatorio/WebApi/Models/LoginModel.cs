using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class LoginModel : Model<User, LoginModel>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public LoginModel() { }
        public LoginModel(User user)
        {
            SetModel(user);
        }

        public override User ToEntity() => new User()
        {
            Email = this.Email,
            Password = this.Password
        };

        protected override LoginModel SetModel(User entity)
        {
            this.Email = entity.Email;
            this.Password = entity.Password;
            return this;
        }
    }
}