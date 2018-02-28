using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CB.IntegrationService.StandardDataSet;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using CB.IntegrationService.ApiModels;
using CB.IntegrationService.ApiClient.Client;
using System.Net;
using CB.IntegrationService.ApiClient.Api;
using CommunityBrands.Demo.Tads.Models;
using CommunityBrands.Demo.Tads.Utils;
using CB.IntegrationService.StandardDataSet.Models;
using EducationBrands.Demo.Tads.ServiceClasses;
using System.Text;
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
        public IHttpActionResult Notification([FromBody]ProductNotificationRequest productNotificationRequest)
        {
            RequestTimeStamp = DateTime.Now;
            int statusCode = 0;

            try
            {
                ProcessRequest(productNotificationRequest);
                statusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                return InternalServerError(ex);
            }

            //Acknowlege the hub, whether the requset is processed successfully or not
            AcknowledgeEbis(productNotificationRequest.EventToken, statusCode);
            return Ok("Processed successfully");
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

            //Constructing header for the acknowledge request
            Dictionary<string, string> dicHeaders = new Dictionary<string, string>();
            //Product Id , Id for Tads in CBIS database
            //pwd : Credentials for communication with CBIS
            string prodId = "2";
            string pwd = "xxxyyyzzz";
            string basicCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(prodId + ":" + pwd));
            dicHeaders.Add("Authorization", "Basic " + basicCredentials);
            dicHeaders.Add("ProductId", "2");
            dicHeaders.Add("ProductName", "Tads");
            dicHeaders.Add("ProductSecret", "xxxyyyzzz");

            Configuration conf = new Configuration();
            conf.DefaultHeader = dicHeaders;
            //Send acknowledge notification to EBIS
            DataExchangeApi instance = new DataExchangeApi(conf);
            instance.NotificationAcknowledge(notificationAckRequest);
        }
    }
}
