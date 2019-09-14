using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerLogic;

namespace ServerConsole
{
    class ServerConsole
    {
        Server server = new Server();
        static void Main(string[] args)
        {
            Server.Connect();

        }
    }
}
