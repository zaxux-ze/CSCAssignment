using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Drawing;

namespace Task7.Controllers
{
    public class ImageRecognitionController : ApiController
    {
        private static string subscriptionKey = "XXXX";
        private static string endpoint = "XXXX";

        private static Account account = new Account(
                "XXXX",
                "XXXX",
                "XXXX");

        [HttpPost]
        [Route("api/v1/performImageRecognition")]
        public async Task<object> performImageRecognition()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                string fileName = "";

                await Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith(o =>
                            {
                                var fileContent = provider.Contents.SingleOrDefault();

                                if (fileContent != null)
                                {
                                    fileName = fileContent.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                                }
                            });

                byte[] file = await provider.Contents[0].ReadAsByteArrayAsync();

                if (!IsValidImage(file))
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Please upload a file!");
                }

                Stream fileStream = new MemoryStream(file);

                Cloudinary cloudinary = authCloudinary();

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, fileStream)
                };
                var uploadResult = cloudinary.Upload(uploadParams);

                var predictedResult = await predictImg(uploadResult.SecureUrl, uploadResult.PublicId);

                return new { result = predictedResult };
            }

            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }

        }

        public static bool IsValidImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                    Image.FromStream(ms);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public HttpResponseMessage removeImg(string publicId)
        {
            Cloudinary cloudinary = authCloudinary();

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = cloudinary.Destroy(deletionParams);
            if (deletionResult.Result.Equals("ok"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        public static Cloudinary authCloudinary()
        {
            Cloudinary cloudinary = new Cloudinary(account);
            return cloudinary;
        }

        public async Task<object> predictImg(Uri secureUrl, string publicId)
        {
            string urlLink = secureUrl.ToString();

            ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

            var obj = await AnalyzeImageUrl(client, urlLink);

            var obj2 = await ReadFileUrl(client, urlLink);

            HttpResponseMessage msg = removeImg(publicId);

            if ((int)msg.StatusCode == 200)
            {
                return new { tagData = obj, ocrData = obj2 };
            }

            else
            {
                return new { message = msg };
            }
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        public static async Task<object> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {
            Dictionary<string, double> tagResult = new Dictionary<string, double>();

            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, features);

            foreach (var tag in results.Tags)
            {
                tagResult.Add(tag.Name, tag.Confidence);
            }

            return new { tagResult = tagResult };
        }

        public static async Task<object> ReadFileUrl(ComputerVisionClient client, string urlFile)
        {

            List<object> lineData = new List<object>();

            var textHeaders = await client.ReadAsync(urlFile, language: "en");
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult results;
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    lineData.Add(line);
                }
            }
            return new { ocrData = lineData };
        }


    }
}
