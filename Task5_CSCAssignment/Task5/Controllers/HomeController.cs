﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Task5.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        public ActionResult UploadImage()
        {
            return View();
        }

        public ActionResult ShortenLink()
        {
            return View();
        }

        public ActionResult Talent()
        {
            return View();
        }
    }
}
