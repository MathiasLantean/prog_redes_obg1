using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class UserModel : Model<User, UserModel>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public UserModel() { }
        public UserModel(User user)
        {
            SetModel(user);
        }

        public override User ToEntity() => new User()
        {
            Email = this.Email,
            Password = this.Password
        };

        protected override UserModel SetModel(User entity)
        {
            this.Email = entity.Email;
            this.Password = entity.Password;
            return this;
        }
    }
}