using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CB.IntegrationService.ApiClient.Client;
using CommunityBrands.Demo.Ravenna.Utils;
using CommunityBrands.Demo.Ravenna.Models;
using CB.IntegrationService.StandardDataSet.Models;
using CB.IntegrationService.ApiClient;
using System.Net;
using CB.IntegrationService.Models;
using CB.IntegrationService.ApiClient.Model;

namespace EducationBrands.Demo.Ravenna.Controllers
{
    public class CBISMessage
    {
        /////// <summary>
        /////// Gets or sets the institution identifier.
        /////// </summary>
        /////// <value>
        /////// The institution identifier.
        /////// </value>
        public string InstitutionId { get; set; }

        /// <summary>
        /// Gets or sets the cb institution identifier.
        /// </summary>
        /// <value>
        /// Global CB institution id â€“ if this value is provided, we can avoid a lookup
        /// </value>
        public string CbInstitutionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>
        /// The name of the event.
        /// </value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the message identifier.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the origin.
        /// </summary>
        /// <value>
        ///  Product code corresponding to the request(message) generator
        /// </value>
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The standard model name of the payload (validations, etc will be done against this)
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the version. Version for different models
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Dictionary<string, string> Version { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The actual payload
        /// </value>
        public Object Data { get; set; }
    }

    public class MemberController : Controller
    {
        // GET: Member
        public ActionResult Index(int skip=0, int take=100)
        {
            DBHandler memHandler = new DBHandler();
            List<MemberModel> LstMem = memHandler.GetMembers(skip,take);
            Session["LstMembers"] = LstMem;
            Session["skip"] = skip;
            Session["take"] = take;
            return View(LstMem);
        }

        public ActionResult Acknowledge()
        {
            Dictionary<string, string> LstAck = new Dictionary<string, string>();
            if (HttpContext.Application["AckReq"] != null)
            {
                LstAck = (Dictionary<string,string>)HttpContext.Application["AckReq"];
            }
            return View(LstAck);
        }

        public ActionResult Next(int skip = 0, int take = 100)
        {
            DBHandler memHandler = new DBHandler();
            List<MemberModel> LstMem = memHandler.GetMembers(skip+100, take+100);
            Session["LstMembers"] = LstMem;
            Session["skip"] = skip + 100;
            Session["take"] = take+100;
            return View("Index", LstMem);
        }

        public ActionResult Previous(int skip = 0, int take = 100)
        {
            if (skip == 0)
            {
                ViewBag.ErrorMessage = "Operation not allowed";
                return View("Index", (List<MemberModel>)Session["LstMembers"]);
            }
            DBHandler memHandler = new DBHandler();
            List<MemberModel> LstMem = memHandler.GetMembers(skip - 100, take - 100);
            Session["LstMembers"] = LstMem;
            Session["skip"] = skip - 100;
            Session["take"] = take - 100;
            return View("Index", LstMem);
        }

        [HttpGet]
        public PartialViewResult GetMember(int id)
        {
           List<MemberModel> lstMdl= (List<MemberModel>)Session["LstMembers"];
            int skip = Session["skip"] == null ? 0 : (int)Session["skip"];
            int take = Session["take"] == null ? 0 : (int)Session["take"];
            if (lstMdl == null)
            {
                DBHandler memHandler = new DBHandler();
                lstMdl = memHandler.GetMembers(skip,take);
            }
            MemberModel mem = lstMdl.FirstOrDefault(l=>l.MemberId==id);
            return PartialView("_UserInfo", mem);

        }

        // GET: Member/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Member/Create
        public ActionResult Create()
        {
            return View();
        }

        // GET: Member/Create
        public ActionResult SendData()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetPayload(long[] IsSelected)
        {
            List<MemberModel> lstMembers = (List<MemberModel>)Session["LstMembers"];
            if (IsSelected.Length > 0)
            {
                try
                {
                    List<Person> StdModel = new List<Person>();
                    MapToStandardDataSet(IsSelected, lstMembers, StdModel);
                    Dictionary<string, string> version = new Dictionary<string, string>();
                    version.Add("Person", "1.0.0");
                    if (StdModel != null || StdModel.Count > 0)
                    {
                        // Send the data
                        var jsonSerialiser = new JavaScriptSerializer();

                        CBISMessage publEvent = new CBISMessage()
                        {
                            CbInstitutionId = "7a804094-283f-11e8-9cea-025339e5fa76",
                            MessageId = Guid.NewGuid().ToString(),
                            Model = "Person",
                            Version = version,
                            Data = StdModel,
                            EventName = "ApplicantAdmitted",
                            InstitutionId = "1001",
                            MessageType = MessageType.Notification.ToString(),
                            Origin = "RAVENNA"
                        };
                        return new JsonResult() { Data = publEvent, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Error: " + ex.Message;
                    return new JsonResult() { Data =  ex.Message , JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            return new JsonResult() { Data = "No Data", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult SendData(long[] isSelected)
        {
            List<MemberModel> lstMembers = (List<MemberModel>)Session["LstMembers"];
            try
            {
                List<Person> StdModel = new List<Person>();
                MapToStandardDataSet(isSelected, lstMembers, StdModel);
                
                CBISResponse publishResponse = PublishToEbis(StdModel);
                ViewBag.SuccessMessage = "Selected members Successfully sent to TADS.";
                if (publishResponse.ResponseCode == HttpStatusCode.Accepted.ToString())
                {
                    ViewBag.SuccessMessage += "Published successfully";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
            }
            return View("Index", lstMembers);

        }

        private CBISResponse PublishToEbis(List<Person> StdModel)
        {
            if (StdModel != null || StdModel.Count > 0)
            {
                // Send the data
                var jsonSerialiser = new JavaScriptSerializer();
                Dictionary<string, string> dicVer = new Dictionary<string, string>();
                dicVer.Add("Person", "1.0.0");
                CB.IntegrationService.ApiClient.Model.CBISMessage publEvent = new CB.IntegrationService.ApiClient.Model.CBISMessage()
                {
                    CbInstitutionId = "7a804094-283f-11e8-9cea-025339e5fa76",
                    MessageId = new Guid().ToString(),
                    Model = typeof(Person).ToString(),
                    Data = jsonSerialiser.Serialize(StdModel),
                    Version = dicVer,
                    EventName = "ApplicantAdmitted",
                    InstitutionId= "1001",
                    MessageType= MessageType.Notification.ToString(),
                    Origin="RAVENNA"
                };
                
                Configuration conf = new Configuration()
                {
                    AuthenticationSecretKey = "ZTI1MjA1NWItMjgzZS0xMWU4LTljZWEtMDI1MzM5ZTVmYTc2OnJhdmVubmFQYXNzd29yZA=="
                };

                DataExchangeApi dataExchangeApi = new DataExchangeApi(conf);
                CBISResponse publishResponse = dataExchangeApi.NotificationPublish(publEvent);
                return publishResponse;
            }
            return null;
        }

        private void MapToStandardDataSet(long[] IsSelected, List<MemberModel> lstMembers, List<Person> StdModel)
        {
            foreach (long key in IsSelected)
            {
                MemberModel ravennaMember = lstMembers.FirstOrDefault(m => m.MemberId == key);
                if (ravennaMember != null)
                {
                    Person member = new Person();
                    member.PersonId = ravennaMember.MemberId.ToString();
                    member.FirstName = ravennaMember.FirstName;
                    member.LastName = ravennaMember.LastName;
                    //just for mapping, not added in DB.hence simply adding M, F for alternate members.
                    ravennaMember.Gender = "M";
                    if (key / 2 == 0)
                    {
                        ravennaMember.Gender = "F";
                    }
                    member.Gender = ravennaMember.Gender;
                    DateTime Date = DateTime.MinValue;
                    DateTime.TryParse(ravennaMember.DOB, out Date);
                    member.BirthDate = Date;
                    member.Grade = new Grade();
                    member.Grade.Name = ravennaMember.Grade;
                    member.Households = new List<Household>();
                    if (ravennaMember.HouseHold != null)
                    {
                        Household huseHld = new Household();
                        huseHld.Addresses = new List<Adddress>();
                        Adddress add1 = new Adddress();
                        add1.Line1 = ravennaMember.HouseHold.Address.Street;
                        add1.City = ravennaMember.HouseHold.Address.City;
                        add1.State = ravennaMember.HouseHold.Address.State;
                        add1.Country = ravennaMember.HouseHold.Address.Country;
                        add1.PostalCode = ravennaMember.HouseHold.Address.Zip;
                        huseHld.Addresses.Add(add1);
                        member.Households.Add(huseHld);
                        if (ravennaMember.HouseHold.Associations != null)
                        {
                            member.Contacts = new List<Contact>();
                            if (ravennaMember.HouseHold.Associations.Count > 0)
                            {
                                for (int i = 0; i < ravennaMember.HouseHold.Associations.Count; i++)
                                {
                                    Contact contct = new Contact();
                                    contct.FirstName = ravennaMember.HouseHold.Associations[i].FirstName;
                                    contct.LastName = ravennaMember.HouseHold.Associations[i].LastName;
                                    contct.MiddleName = ravennaMember.HouseHold.Associations[i].MiddleName;
                                    contct.Phone = ravennaMember.HouseHold.Associations[i].PrimaryPhone;
                                    contct.Email = ravennaMember.HouseHold.Associations[i].PrimaryEmail;
                                    contct.CustodyType = ravennaMember.HouseHold.Associations[i].CustodyStatus;
                                    contct.RelationShipType = ravennaMember.HouseHold.Associations[i].Relationship;
                                    member.Contacts.Add(contct);
                                    if (i == 0)
                                    {
                                        huseHld.Name = contct.LastName;
                                        huseHld.Phones = new List<Phone>();
                                        huseHld.Phones.Add(new Phone() { Number = contct.Phone });
                                        huseHld.EmailAddresses = new List<Email>();
                                        huseHld.EmailAddresses.Add(new Email() { EmailId = contct.Email });
                                    }
                                }
                            }
                        }
                    }
                    if (ravennaMember.FAGrantAmount != null && ravennaMember.FAGrantAmount > 0)
                    {
                        member.FinancialAid = new FinancialAid();
                        member.FinancialAid.FinalAward = decimal.Parse(ravennaMember.FAGrantAmount.ToString());
                    }
                    StdModel.Add(member);
                }
            }
        }

        // POST: Member/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Member/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Member/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Member/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Member/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
