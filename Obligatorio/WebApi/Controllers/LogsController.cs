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
    [RoutePrefix("api/logs")]
    public class LogsController : ApiController
    {
        private ILogService _logService;

        public LogsController()
        {
            string ip = ConfigurationManager.AppSettings["RemotingIP"];
            string port = ConfigurationManager.AppSettings["RemotingPort"];
            string remoteUri = ConfigurationManager.AppSettings["RemotingUri"];

            _logService = (ILogService)Activator.GetObject(
                typeof(ILogService),
                $"tcp://{ip}:{port}/{remoteUri}");
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            try
            {
                var logs = _logService.GetLogs();

                if (logs.Count() == 0)
                {
                    return Ok("No hay logs para mostrar.");
                }
                return Ok(logs);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("{type}")]
        public IHttpActionResult GetByType(string type)
        {
            try
            {
                int logtype = Int32.Parse(type);
                var logs = _logService.GetLogsByType(logtype);

                if (logs.Count() == 0)
                {
                    return Ok("No hay logs de este tipo para mostrar.");
                }
                return Ok(logs);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}