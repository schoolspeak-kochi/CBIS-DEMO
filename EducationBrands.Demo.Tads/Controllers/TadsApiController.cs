using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using EducationBrands.Demo.Tads.Models;
using CB.IntegrationService.StandardDataSet;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using CB.IntegrationService.ApiModels;
using CB.IntegrationService.ApiClient.Client;
using System.Net;
using CB.IntegrationService.ApiClient.Api;

namespace EducationBrands.Demo.Controllers
{
    [RoutePrefix("TadsApi")]
    public class TadsAPIController : ApiController
    {
        public DateTime RequestTimeStamp { get; set; }
        [Route("Ping")]
        [HttpGet]
        public IHttpActionResult Ping()
        {
            return Ok("Pong Ravenna @ " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
        }

        [Route("PingPost")]
        [HttpPost]
        public IHttpActionResult PingPost(string data)
        {
            return Ok("PongPost Ravenna @ " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
        }

        [Route("Notification")]
        [HttpPost]
        public IHttpActionResult Notification([FromBody]ProductNotificationRequest productNotificationRequest)
        {
            //int statusCode = 0;;
            try
            {
                RequestTimeStamp = DateTime.Now;
                ProcessRequest(productNotificationRequest);
                //statusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                //statusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Ok("Processed successfully");

            // AcknowledgeEbis(productNotificationRequest.EventToken, statusCode);
        }

        /// <summary>
        /// Processing the request
        /// </summary>
        /// <param name="ebEventPayload"></param>
        [NonAction]
        public static void ProcessRequest(ProductNotificationRequest ebEventPayload)
        {
            if (ebEventPayload == null)
                return;

            //Deserilize json string as class object
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new JavaScriptSerializer();
            MembersCollection deserilized = serializer.Deserialize<MembersCollection>(ebEventPayload.Payload);

            string ebInstitutionId = ebEventPayload.EbInstitutionId;
            string InstitutionName = ebEventPayload.InstitutionName;
            List<Applicant> applicants = new List<Applicant>();

            foreach (Member mem in deserilized.Members)
            {
                Applicant applicant = new Applicant();
                applicant.SchoolName = InstitutionName;
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
            }

            DBHelper.InsertApplicantstoDB(applicants);
        }

        /// <summary>
        /// Send acknowledge information to EBISs
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="statusMessage"></param>
        private void AcknowledgeEbis(string eventToken, int statusCode)
        {
            NotificationAcknowledgeRequest notificationAckRequest = new NotificationAcknowledgeRequest()
            {
                AcknowledgeTimestamp = DateTime.Now.ToString(),
                RequestTimestamp = RequestTimeStamp.ToString(),
                EventToken = eventToken,
                StatusCode = statusCode.ToString(),
                StatusMessage = statusCode == (int)HttpStatusCode.OK ? "Processed successfully" : "Operation failed"
            };

            Dictionary<string, string> dicHeaders = new Dictionary<string, string>();
            dicHeaders.Add("ProductId", "2");
            dicHeaders.Add("ProductName", "Tads");
            dicHeaders.Add("ProductSecret", "xxxyyyzzz");
            Configuration conf = new Configuration();
            conf.DefaultHeader = dicHeaders;

            DataExchangeApi instance = new DataExchangeApi(conf);
            instance.NotificationAcknowledgeEvent(notificationAckRequest);
        }
    }
}
