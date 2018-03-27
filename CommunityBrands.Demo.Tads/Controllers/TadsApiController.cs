using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CB.IntegrationService.StandardDataSet;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
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
        public IHttpActionResult Notification([FromBody]CBISMessage cbisMessage)
        {
            try
            {

                if (cbisMessage == null || cbisMessage.Data == null)
                {
                    throw new Exception("Payload is empty");
                }

                JToken token = cbisMessage.Data;
                List<Person> lstPerson = new List<Person>();

                if (token is JArray)
                {
                    lstPerson = token.ToObject<List<Person>>();
                }
                else if (token is JObject)
                {
                    lstPerson.Add(token.ToObject<Person>());
                }

                List<Applicant> applicants = new List<Applicant>();
                List<CBISResult> lstCBISResult = new List<CBISResult>();

                foreach (Person mem in lstPerson)
                {
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

                        List<string> errors = this.ValidateData(applicant);
                        if (errors.Count > 0)
                        {

                            lstCBISResult.Add(new CBISResult
                            {
                                Id = mem.PersonId,
                                ResultType = "E_UNPROCESSABLE_RECORD",
                                Errors = errors
                            });
                        }
                        else
                        {
                            applicants.Add(applicant);

                            lstCBISResult.Add(new CBISResult
                            {
                                Id = mem.PersonId,
                                ResultType = "SUCCESS"
                            });
                        }
                    }
                }

                if (!DBHelper.InsertApplicantstoDB(applicants))
                {
                    throw new Exception("DB acess failed");
                }

                CBISMessage cbisMessage_Response = new CBISMessage
                {
                    CbInstitutionId = cbisMessage.CbInstitutionId,
                    EventName = cbisMessage.EventName,
                    MessageId = cbisMessage.MessageId,
                    MessageType = "NotificationResponse",
                    Model = "CBISResult",
                    Origin = "Tads",
                    Data = JToken.FromObject(lstCBISResult)
                };

                return Ok(cbisMessage_Response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private List<string> ValidateData(Applicant applicant)
        {
            List<string> errors = new List<string>();

            //Validating gender
            switch (applicant.Gender.ToUpper())
            {
                case "M":
                case "MALE":
                case "F":
                case "FEMALE":
                    break;

                default:
                    errors.Add("Gender is invalid");
                    break;
            }
            return errors;
        }
    }
}
