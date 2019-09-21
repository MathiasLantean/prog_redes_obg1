using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentFactory
{
    public class ContentCommand : Content
    {
        
        public string init = "{";
        public string action = "action: ";
        public string type = "content-type: ";
        public string data = "data: { }";
        public string end = "}";

        public void addAction(string action)
        {
            this.action += action;
        }
        public void addType(string type)
        {
            this.type += type;
        }

        public void addData(string data) {
           // data
        }

        public string getCommand() {
            return "";
        }
    }
}
