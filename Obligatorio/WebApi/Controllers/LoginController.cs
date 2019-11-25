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
    [RoutePrefix("api/login")]
    public class LoginController : ApiController
    {
        private IRemotingService _remotingService;

        public LoginController()
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
        public IHttpActionResult Post([FromBody]LoginModel model)
        {
            try
            {
                Guid token = _remotingService.TeacherLogin(model.ToEntity());
                return Created("Get", "Su token de acceso es: " + token);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}