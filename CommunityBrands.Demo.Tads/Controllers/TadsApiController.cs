﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CB.IntegrationService.StandardDataSet;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using CB.IntegrationService.ApiClient;
using System.Net;
using CommunityBrands.Demo.Tads.Models;
using CommunityBrands.Demo.Tads.Utils;
using CB.IntegrationService.StandardDataSet.Models;
using EducationBrands.Demo.Tads.ServiceClasses;
using System.Text;
using CB.IntegrationService.ApiClient.Model;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace CommunityBrands.Demo.Controllers
{
    [RoutePrefix("TadsApi")]
    public class TadsAPIController : ApiController
    {
        public DateTime RequestTimeStamp { get; set; }
        [Route("Ping")]
        [HttpGet]
        public IHttpActionResult Ping()
        {
            return Ok("Pong TadsDemo @ " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
        }

        [Route("PingPost")]
        [HttpPost]
        public IHttpActionResult PingPost(string data)
        {
            return Ok("PongPost TadsDemo @ " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
        }

       // [TadsAuthorization]
     //   [Authorize]
        [Route("Notification")]
        [HttpPost]
        public HttpResponseMessage Notification([FromBody]CBISMessage cbisMessage)
        {
            try
            {
               return ProcessRequest(cbisMessage);

            }
            catch (Exception ex)
            {
                //log error
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Processing the request
        /// </summary>
        /// <param name="ebEventPayload"></param>
        [NonAction]
        public static HttpResponseMessage ProcessRequest(CBISMessage cbisMessage)
        {
            if (cbisMessage == null)
                return null;

            List<Applicant> applicants = new List<Applicant>();
            List<CBISResult> lstCBISResult = new List<CBISResult>();
            foreach (JObject jObject in cbisMessage.Data.Children<JObject>())
            {
                Member mem = jObject.ToObject<Member>();
                if (mem != null)
                {
                    Applicant applicant = new Applicant();
                    applicant.FirstName = mem.FirstName;
                    applicant.LastName = mem.LastName;
                    applicant.MiddleName = mem.MiddleName;
                    applicant.Gender = mem.Gender;
                    applicant.DOB = mem.BirthDate.ToString();

                    if (mem.Households != null && mem.Households.Count > 0)
                    {
                        Household houseHold = mem.Households[0];
                        applicant.HouseholdName = houseHold.Name;
                        if (houseHold.Phones != null && houseHold.Phones.Count > 0)
                            applicant.HouseholdPhone = houseHold.Phones[0].Number;
                        if (houseHold.EmailAddresses != null && houseHold.EmailAddresses.Count > 0)
                            applicant.HouseholdEmail = houseHold.EmailAddresses[0].EmailId;
                    }

                    if (mem.Grade != null)
                        applicant.Grade = mem.Grade.Name;
                    applicants.Add(applicant);

                    lstCBISResult.Add(new CBISResult
                    {
                        Id = "0",
                        ResultType = "SUCCESS"
                    });
                }
            }

            DBHelper.InsertApplicantstoDB(applicants);

            CBISMessage cbisMessage_Response = new CBISMessage
            {
                CbInstitutionId = cbisMessage.CbInstitutionId,
                EventName = cbisMessage.EventName,
                MessageId = cbisMessage.MessageId,
                MessageType = "NotificationResponse",
                Model = "CBISResult",
                Data = JContainer.FromObject(lstCBISResult)
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(new JavaScriptSerializer().Serialize(cbisMessage_Response));
            return response;
        }
    }
}
