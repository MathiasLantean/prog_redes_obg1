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
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;

namespace WebApi.Controllers
{
    [RoutePrefix("api/teacher")]
    public class TeacherController : ApiController
    {
        private IRemotingService _remotingService;

        public TeacherController()
        {
            string ip = ConfigurationManager.AppSettings["RemotingIP"];
            string port = ConfigurationManager.AppSettings["RemotingPort"];
            string remoteUri = ConfigurationManager.AppSettings["RemotingUri"];

            _remotingService = (IRemotingService)Activator.GetObject(
                typeof(IRemotingService),
                $"tcp://{ip}:{port}/{remoteUri}");
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody]TeacherModel model)
        {
            try
            {
                var teacher = _remotingService.AddTeacher(TeacherModel.ToEntity(model));
                return Created("Get", TeacherModel.ToModel(teacher));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("{email}", Name = "GetTeacher")]
        public IHttpActionResult Get(string email)
        {
            try
            {
                var teacher = _remotingService.GetTeacher(email);

                if (teacher == null)
                {
                    return Ok("No existe un docente con este correo electrónico.");
                }
                return Ok(TeacherModel.ToModel(teacher));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}