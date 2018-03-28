using CB.IntegrationService.ApiClient.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

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
        [Route("notification")]
        [HttpPost]
        public void AcknowledgeNotification([FromBody] CB.IntegrationService.ApiClient.Model.CBISMessage ackModel)
        {
            JToken token = ackModel.Data;
            List<CBISResult> lstREsults = new List<CBISResult>();
            if (token is JArray)
            {
                lstREsults = token.ToObject<List<CBISResult>>();
            }

            var serializer = new JavaScriptSerializer();
            Dictionary<string, string> lstEventToken = HttpContext.Current.Application["AckReq"] == null ? new Dictionary<string, string>() : (Dictionary<string, string>)HttpContext.Current.Application["AckReq"];
            lstEventToken.Add(ackModel.MessageId, ackModel.ToString());
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