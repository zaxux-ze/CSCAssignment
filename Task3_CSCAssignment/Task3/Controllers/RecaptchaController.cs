using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Task3.Models;

namespace Task3.Controllers
{
    public class RecaptchaController : ApiController
    {
        [HttpPost]
        [Route("api/home/recaptcha")]
        public HttpResponseMessage CheckRecaptcha(ReCaptchaModel model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            else if (!ReCaptchaPassed(model.GoogleCaptchaToken))
            {
                ModelState.AddModelError(string.Empty, "You failed the CAPTCHA.");
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "You passed the CAPTCHA");
            }
        }

        public static bool ReCaptchaPassed(string gRecaptchaResponse)
        {
            HttpClient httpClient = new HttpClient();

            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret=XXXX&response={gRecaptchaResponse}").Result;

            if (res.StatusCode != HttpStatusCode.OK)
                return false;

            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);

            if (JSONdata.success != "true")
                return false;

            return true;
        }
    }
}
