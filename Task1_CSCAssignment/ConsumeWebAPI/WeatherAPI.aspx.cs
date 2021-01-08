using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ConsumeWebAPI
{
    public partial class WeatherAPI : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://api.data.gov.sg/v1/environment/2-hour-weather-forecast");
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "application/json";
            webRequest.Method = "GET";

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            JsonDocument jsonObj = JsonDocument.Parse(webResponse.GetResponseStream());
            JsonElement jsonElm = jsonObj.RootElement;
            Response.Write(jsonElm.ToString());
        }
    }
}