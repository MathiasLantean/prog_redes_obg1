using Domain;
using RemotingServiceInterface.RemotingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotingServiceInterface
{
    public interface IRemotingService
    {
        Guid TeacherLogin(User login);
        Teacher AddTeacher(Teacher teacher);
        Teacher GetTeacher(string email);
        List<string> GetCoursesWithTasksToCorrect(Guid token);
        List<string> GetStudentsToCorrect(string course, string task);
        void ScoreStudent(Guid token, string courseName, string taskName, int studentNumber, int score);
    }
}
