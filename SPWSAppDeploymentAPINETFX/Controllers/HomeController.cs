using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPWSAppDeploymentAPINETFX.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Monitoring()
        {
            ViewBag.Title = "Monitoring";

            return View();
        }

        public ActionResult Settings()
        {
            ViewBag.Title = "Settings";
            return View();
        }

        public ActionResult Applications()
        {
            ViewBag.Title = "Applications";

            return View();
        }
    }
}
