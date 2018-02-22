using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CB.IntegrationService.StandardDataSet;
using System.IO;
using System.Web.Script.Serialization;
using CommunityBrands.Demo.Tads.Utils;

namespace CommunityBrands.Demo.Tads.Controllers
{
    public class ApplicationsController : Controller
    {
        // GET: Home
        public ActionResult List(int pageNo = 0)
        {
            int totalCount = 0;
            var model = DBHelper.GetAllApplicants(out totalCount, pageNo);
            ViewBag.totalCount = totalCount;
            ViewBag.Next = pageNo + 1;
            if (pageNo > 0)
                ViewBag.Previous = pageNo - 1;
            else
                ViewBag.Previous = 0;

            return View(model);
        }
    }
}
