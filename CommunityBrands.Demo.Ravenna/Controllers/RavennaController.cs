using CB.IntegrationService.ApiClient.Model;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace EducationBrands.Demo.Ravenna.Controllers
{
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
        [Route("Notification")]
        [HttpPost]
        public void AcknowledgeNotification([FromBody]CBISMessage ackModel)
        {
            if (ackModel.Data!= null)
            {
                CBISResult errors = (CBISResult)ackModel.Data;
                if (errors != null && errors.Errors.Count > 0)
                {
                    List<string> lstErrors = HttpContext.Current.Application["AckError"] == null ? new List<string>() : (List<string>)HttpContext.Current.Application["AckError"];
                    lstErrors.AddRange(errors.Errors);
                    HttpContext.Current.Application["AckError"] = lstErrors;
                }
            }
            List<string> lstEventToken = HttpContext.Current.Application["AckReq"] == null ? new List<string>() : (List<string>)HttpContext.Current.Application["AckReq"];
            lstEventToken.Add(ackModel.MessageId);
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