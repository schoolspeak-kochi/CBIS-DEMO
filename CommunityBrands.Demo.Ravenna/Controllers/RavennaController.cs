﻿using CB.IntegrationService.ApiClient.Model;
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
        [Route("Notification")]
        [HttpPost]
        public void AcknowledgeNotification([FromBody]CBISMessage ackModel)
        {
            var serializer = new JavaScriptSerializer();
            Dictionary<string, string> lstEventToken = HttpContext.Current.Application["AckReq"] == null ? new Dictionary<string, string>() : (Dictionary<string, string>)HttpContext.Current.Application["AckReq"];
            if (ackModel.Data!= null)
            {
                if (ackModel.Data!= null && string.IsNullOrEmpty(ackModel.Data.ToString()))
                {
                    lstEventToken.Add(ackModel.MessageId, serializer.Serialize(ackModel.Data.ToObject<List<CBISResult>>()));
                    HttpContext.Current.Application["AckReq"] = lstEventToken;
                }
            }
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