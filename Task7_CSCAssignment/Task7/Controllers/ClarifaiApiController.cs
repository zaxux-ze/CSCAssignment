using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Task7.Models;

namespace Task7.Controllers
{
    public class ClarifaiApiController : ApiController
    {
        [HttpPost]
        [Route("api/clarifai/imageRecognition")]
        public object ImageRecogntion(ImageData imgData)
        {

            FileStream file = new FileStream(imgData.ImagePath, FileMode.Open);
            string tagResult = ClarifaiTagging(file);
            string ocrResult = ClarifaiOcr(file);
            return new { tags = tagResult, ocr = ocrResult };
        }

        public static string ClarifaiTagging(Stream imageStream)
        {
            string ACCESS_TOKEN = "XXXX";
            string CLARIFAI_API_URL = "https://api.clarifai.com/v2/models/{model-name}/outputs";

            MemoryStream ms = new MemoryStream();
            imageStream.CopyTo(ms);
            string encodedData = Convert.ToBase64String(ms.ToArray());

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ACCESS_TOKEN);

                HttpContent json = new StringContent(
                    "{" +
                        "\"inputs\": [" +
                            "{" +
                                "\"data\": {" +
                                    "\"image\": {" +
                                        "\"base64\": \"" + encodedData + "\"" +
                                    "}" +
                               "}" +
                            "}" +
                        "]" +
                    "}", Encoding.UTF8, "application/json");

                // Send the request to Clarifai and get a response
                var response = client.PostAsync(CLARIFAI_API_URL, json).Result;

                // Check the status code on the response
                if (!response.IsSuccessStatusCode)
                {
                    // End here if there was an error
                    return null;
                }

                // Get response body
                string body = response.Content.ReadAsStringAsync().Result.ToString();

                return body;
            }
        }

        public static string ClarifaiOcr(Stream imageStream)
        {

            string ACCESS_TOKEN = "XXXX";
            string CLARIFAI_API_URL = "https://api.clarifai.com/v2/models/{model-name}/outputs";

            MemoryStream ms = new MemoryStream();
            imageStream.CopyTo(ms);
            string encodedData = Convert.ToBase64String(ms.ToArray());

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ACCESS_TOKEN);

                HttpContent json = new StringContent(
                    "{" +
                        "\"inputs\": [" +
                            "{" +
                                "\"data\": {" +
                                    "\"image\": {" +
                                        "\"base64\": \"" + encodedData + "\"" +
                                    "}" +
                               "}" +
                            "}" +
                        "]" +
                    "}", Encoding.UTF8, "application/json");

                var response = client.PostAsync(CLARIFAI_API_URL, json).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string body = response.Content.ReadAsStringAsync().Result.ToString();

                return body;
            }
        }

    }
}
