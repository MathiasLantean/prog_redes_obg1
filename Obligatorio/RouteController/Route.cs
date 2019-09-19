using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteController
{
    public enum Action {
        Login
    }

    public enum ContentType
    {
        Json, Student, Admin, Task
    }

    public class ActionDispatcher
    {
        public Action action;
        public ContentType contentType;
        public string data;

        public ActionDispatcher(Action action, ContentType contentType, string data) {
            this.action = action;
            this.contentType = contentType;
            this.data = data;
        }

        public void dispatch(int action)
        {
            Type type = typeof(ActionDispatcher);
            //MyMethodInfo metodo = type.GetMethod();
        }
    }
}
