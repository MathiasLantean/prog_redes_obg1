using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class TeacherModel : Model<Teacher, TeacherModel>
    {
        public UserModel User { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public TeacherModel() { }
        public TeacherModel(Teacher teacher)
        {
            SetModel(teacher);
        }

        public override Teacher ToEntity() => new Teacher()
        {
            User = this.User.ToEntity(),
            Name = this.Name,
            Surname = this.Surname
        };

        protected override TeacherModel SetModel(Teacher entity)
        {
            this.User = new UserModel(entity.User);
            this.Name = entity.Name;
            this.Surname = entity.Surname;
            return this;
        }
    }
}