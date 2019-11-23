using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherInterface;
using Domain;
using ServerLogic;

namespace TeacherImplementation
{
    public class TeacherLogic : TeacherLogicInterface
    {

        private Server serverLogic;

        public TeacherLogic(Server serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public Teacher Add(Teacher teacher)
        {
            try
            {
                return this.serverLogic.AddTeacher(teacher);
            }catch(ArgumentException e)
            {
                throw e;
            }
        }

        public Teacher Get(string email)
        {
            try
            {
                return this.serverLogic.GetTeacher(email);
            }
            catch(ArgumentException e)
            {
                throw e;
            }
        }
    }
}
