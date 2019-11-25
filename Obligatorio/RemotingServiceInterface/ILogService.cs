using Domain;
using RemotingServiceInterface.RemotingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotingServiceInterface
{
    public interface ILogService
    {
        List<string> GetLogs();
        List<string> GetLogsByType(int typeLog);
    }
}
