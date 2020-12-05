using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Task1_CSCAssignment.Controllers
{
    public class WeatherAPIController : Controller
    {
        // GET: WeatherAPI
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WeatherAPIPage()
        {
            ViewBag.Title = "Weather API";

            return View();
        }
    
    }
}