using Microsoft.AspNetCore.Mvc;
using RemotingServiceInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WebApi.Models;
using HttpGetAttribute = System.Web.Mvc.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using HttpPutAttribute = System.Web.Http.HttpPutAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;

namespace WebApi.Controllers
{
    [RoutePrefix("api/tasks")]
    public class StudentTasksController : ApiController
    {
        private IRemotingService _remotingService;

        public StudentTasksController()
        {
            string ip = ConfigurationManager.AppSettings["RemotingIP"];
            string port = ConfigurationManager.AppSettings["RemotingPort"];
            string remoteUri = ConfigurationManager.AppSettings["RemotingUri"];

            _remotingService = (IRemotingService)Activator.GetObject(
                typeof(IRemotingService),
                $"tcp://{ip}:{port}/{remoteUri}");
        }

        [HttpGet]
        [Route("{token}", Name = "GetCoursesWithTasksToCorrect")]
        public IHttpActionResult Get(Guid token)
        {
            try
            {
                var courses = _remotingService.GetCoursesWithTasksToCorrect(token);

                if (courses.Count() == 0)
                {
                    return Ok("No hay cursos con tareas pendientes de corregir.");
                }
                else
                {
                    var studentsToCorrect = new List<string>();
                    foreach (string course in courses)
                    {
                        var courseName = course.Split('&')[0];
                        var tasks = course.Split('&')[1].Split(',');
                        foreach(string task in tasks)
                        {
                            var taskName = task.Split(' ')[0]; 
                            var students = _remotingService.GetStudentsToCorrect(courseName, taskName);
                            foreach (string student in students)
                            {
                                studentsToCorrect.Add("Curso: " + courseName + ", Tarea: " + task + ", Estudiante: " + student);
                            }
                        }
                        
                    }
                    return Ok(studentsToCorrect);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("{token}/{courseName}/{taskName}/{studentNumber}/{scoreS}")]
        public IHttpActionResult Put(Guid token, string courseName, string taskName, string studentNumber, string scoreS)
        {
            try
            {
                int student = Int32.Parse(studentNumber);
                int score = Int32.Parse(scoreS);
                _remotingService.ScoreStudent(token, courseName, taskName, student, score);
                return Created("Get", "Estudiante corregido correctamente.");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}