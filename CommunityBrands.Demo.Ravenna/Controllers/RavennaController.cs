using CB.IntegrationService.ApiModels;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace EducationBrands.Demo.Ravenna.Controllers
{
    [RoutePrefix("RavennaApi")]
    public class RavennaController : ApiController
    {
        [Route("Ping")]
        [HttpGet]
        public IHttpActionResult Ping()
        {
            return Ok("Pong Ravenna @ " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
        }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [Route("acknowledgeNotification")]
        [HttpPost]
        public void AcknowledgeNotification([FromBody]NotificationAcknowledgeRequest ackModel)
        {
            List<string> lstEventToken = HttpContext.Current.Application["AckReq"] == null ? new List<string>():(List<string>) HttpContext.Current.Application["AckReq"];
            lstEventToken.Add(ackModel.EventToken);
            HttpContext.Current.Application["AckReq"] = lstEventToken;
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}