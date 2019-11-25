using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public enum LogType
    {
        CreateStudent,
        CreateTeacher,
        CreateCourse,
        RemoveCourse,
        AddStudentToCourse,
        CorrectTask
    }
    public class Log
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public LogType Type { get; set; }

        public Log() { }

        public Log(DateTime date, string description, int logType) {
            this.Date = date;
            this.Description = description;
            this.Type = (LogType)logType;
        }

        public override string ToString()
        {
            return "Fecha: " + this.Date.ToString("dd/MM/yyyy") + " -> Descripción: "+this.Description;
        }

    }
}
