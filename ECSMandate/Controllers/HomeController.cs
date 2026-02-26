using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class HomeController : AppController
    {
        MandateService mandateService = new MandateService();
        public ActionResult Index()
        { 
            DataTable dtIWMandate = mandateService.GetAllMandateCount("IW", StartUp.DAY_OP_DT);
            DataTable dtOWMandate = mandateService.GetAllMandateCount("OW", StartUp.DAY_OP_DT);

            ViewBag.OWMandate = dtOWMandate;

            return View(dtIWMandate);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}