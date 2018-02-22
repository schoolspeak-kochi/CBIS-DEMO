using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CB.IntegrationService.StandardDataSet;
using EducationBrands.Demo.Ravenna.Models;
using System.Web.Script.Serialization;
using CB.IntegrationService.ApiModels;
using CB.IntegrationService.ApiClient.Client;
using CB.IntegrationService.ApiClient.Api;

namespace EducationBrands.Demo.Ravenna.Controllers
{
    public class MemberController : Controller
    {
        // GET: Member
        public ActionResult Index(int skip=0, int take=100)
        {
            MemberDBHandler memHandler = new MemberDBHandler();
            List<MemberModel> LstMem = memHandler.GetMembers(skip,take);
            Session["LstMembers"] = LstMem;
            Session["skip"] = skip;
            Session["take"] = take;
            return View(LstMem);
        }

        public ActionResult Acknowledge()
        {
            List<string> LstAck = new List<string>();
            if (HttpContext.Application["AckReq"] != null)
            {
                LstAck = (List<string>)HttpContext.Application["AckReq"];
            }
            return View(LstAck);
        }

        public ActionResult Next(int skip = 0, int take = 100)
        {
            MemberDBHandler memHandler = new MemberDBHandler();
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
            MemberDBHandler memHandler = new MemberDBHandler();
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
                MemberDBHandler memHandler = new MemberDBHandler();
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
        public ActionResult SendData(long[] isSelected)
        {
            List<MemberModel> lstMembers = (List<MemberModel>)Session["LstMembers"];
            try
            {
                MembersCollection StdModel = new MembersCollection();
                StdModel.Members = new List<Member>();
                MapToStandardDataSet(isSelected, lstMembers, StdModel);
                PublishEventResponse publishResponse = PublishToEbis(StdModel);
                ViewBag.SuccessMessage = "Selected members Successfully sent to TADS.";
                if (publishResponse != null)
                {
                    ViewBag.SuccessMessage += ". Publish Event Token: " + publishResponse.EbPublishedEventId;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
            }
            return View("Index", lstMembers);

        }

        private PublishEventResponse PublishToEbis(MembersCollection StdModel)
        {
            if (StdModel != null || StdModel.Members.Count > 0)
            {
                // Send the data
                var jsonSerialiser = new JavaScriptSerializer();

                PublishEventRequest publEvent = new PublishEventRequest()
                {
                    AcknowledgementRequired = true,
                    EbInstitutionId = "1",
                    InstitutionName = "Summer wood school",
                    EventName = "StudentsAdmitted",
                    Payload = jsonSerialiser.Serialize(StdModel),
                };
                Dictionary<string, string> dicHeaders = new Dictionary<string, string>();
                dicHeaders.Add("ProductId", "5");
                dicHeaders.Add("ProductName", "Ravenna");
                dicHeaders.Add("ProductSecret", "xxxyyyzzz");
                Configuration conf = new Configuration();
                conf.DefaultHeader = dicHeaders;
                DataExchangeApi instance = new DataExchangeApi(conf);
                PublishEventResponse publishResponse = instance.PublishNotificationEvent(publEvent);
                return publishResponse;
            }
            return null;
        }

        private void MapToStandardDataSet(long[] IsSelected, List<MemberModel> lstMembers, MembersCollection StdModel)
        {
            foreach (long key in IsSelected)
            {
                MemberModel ravennaMember = lstMembers.FirstOrDefault(m => m.MemberId == key);
                if (ravennaMember != null)
                {
                    Member member = new Member();
                    member.FirstName = ravennaMember.FirstName;
                    member.LastName = ravennaMember.LastName;
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
                        huseHld.Addresses = new List<Add>();
                        Add add1 = new Add();
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
                    StdModel.Members.Add(member);
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
