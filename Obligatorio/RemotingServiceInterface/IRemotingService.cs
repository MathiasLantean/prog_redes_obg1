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
        bool TeacherLogin(string mail, string password);
        List<string> GetCoursesWithTasksToCorrect();
        List<string> GetStudentsToCorrect(string course, string task);
        List<string> GetTasksToCorrect(string course);
        void ScoreStudent(string courseName, string taskName, int studentNumber, int score);
    }
}
