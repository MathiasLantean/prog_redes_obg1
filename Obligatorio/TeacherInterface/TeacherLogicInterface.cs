using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherInterface
{
    public interface TeacherLogicInterface
    {
        Teacher Add(Teacher teacher);

        Teacher Get(string email);
    }
}
