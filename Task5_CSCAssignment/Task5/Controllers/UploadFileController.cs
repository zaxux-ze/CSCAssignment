using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Task5.Controllers
{
    public class UploadFileController : ApiController
    {
        private const string bucketName = "zes3version2";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast1;
        private static IAmazonS3 s3Client;

        [HttpPost]
        [Route("api/v1/uploadFileToS3")]
        public async Task<HttpResponseMessage> UploadFileToS3()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            try
            {
                s3Client = new AmazonS3Client(bucketRegion);
                string fileName = "";

                var fileTransferUtility =
                    new TransferUtility(s3Client);

                await Request.Content.ReadAsMultipartAsync(provider).ContinueWith(o =>
                {
                    var fileContent = provider.Contents.SingleOrDefault();

                    if (fileContent != null)
                    {
                        fileName = fileContent.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                    }
                });

                byte[] file = await provider.Contents[0].ReadAsByteArrayAsync();

                Stream fileStream = new MemoryStream(file);

                fileTransferUtility.Upload(fileStream, bucketName, fileName);

                string objectUrl = String.Format("https://{0}.s3.{1}.amazonaws.com/{2}", bucketName, bucketRegion.SystemName, fileName);

                dynamic shortenUrlLink = await ShortenUrl(objectUrl);

                if ((int)shortenUrlLink.statusCode == 200)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { url = shortenUrlLink.urlLink, statusCode = (int)shortenUrlLink.statusCode == 200 });
                }
                else {
                    HttpStatusCode statusCode = shortenUrlLink.statusCode;
                    string message = shortenUrlLink.message;
                    return Request.CreateResponse(statusCode, message);
                }

                
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }


        public async Task<object> ShortenUrl(string url)
        {
            string _bitlyToken = "XXXX";
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                "https://api-ssl.bitly.com/v4/shorten")
            {
                Content = new StringContent($"{{\"long_url\":\"{url}\"}}",
                                                Encoding.UTF8,
                                                "application/json")
            };

            try
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bitlyToken);

                var response = await client.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) {
                    return new { statusCode = response.StatusCode, message = "Unable to shorten link, please try again later." };
                }
                    
                var responsestr = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);
                return new { urlLink = jsonResponse["link"], statusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
