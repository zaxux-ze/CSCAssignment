using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Task3.Services
{
    public class Captcha
    {

        public bool VerifyResponse(string recaptchaString)
        {
            string requestUrl = "https://www.google.com/recaptcha/api/siteverify";
            string recaptchaSecretKey = "6LfzfAQaAAAAAESKIl9iUWklLdBiYkIx_59hcH4r";

            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

            request.Timeout = 15 * 1000;
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded";

            string postData = string.Format("secret={0}&response={1}", recaptchaSecretKey, recaptchaString);
            byte[] buffer = Encoding.Default.GetBytes(postData);

            using (Stream stream = request.GetRequestStream()) {
                stream.Write(buffer, 0, buffer.Length);
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            JsonDocument jsonDoc = JsonDocument.Parse(response.GetResponseStream());

            JsonElement jsonFile = jsonDoc.RootElement;

            double humanScore = jsonFile.GetProperty("score").GetDouble();

            if (humanScore < 0.5)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

    }
}