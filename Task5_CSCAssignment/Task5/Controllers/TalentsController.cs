using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Task5.Models;

namespace Task5.Controllers
{
    public class TalentsController : ApiController
    {

        private const string bucketName = "zes3";
        //private const string objectKey = "all_images/Barot_Bellingham_tn.jpg";
        // Specify how long the presigned URL lasts, in hours
        private const double timeoutDuration = 12;
        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast1;
        private static IAmazonS3 s3Client;

        static readonly TalentRepository repository = new TalentRepository();
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/talents")]
        public IEnumerable<Talent> GetAllTalents()
        {
            return repository.GetAll();
        }

        [Route("api/talents/{id:int}")]
        public Talent GetTalent(int id)
        {
            Talent item = repository.Get(id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return item;
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        [Route("api/talents/uploadImage")]
        public void UploadImage()
        {
            UploadObject amazonS3 = new UploadObject();
            amazonS3.UploadFile();
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        [Route("api/talents/shortenLink")]
        public async Task<object> ShortenLink(UrlLink linkData)
        {
            Double timeoutDuration = 12;
            ShortenLink shorten = new ShortenLink();
            s3Client = new AmazonS3Client(bucketRegion);
            string urlString = GeneratePreSignedURL(timeoutDuration, linkData.KeyName);
            string url = await shorten.ShortenUrl(urlString);
            return new { keyName = linkData.Key, link = url };
        }

        [HttpGet]
        [Route("api/talents/getBucketObject")]
        public List<string> GetBucketObject()
        {
            s3Client = new AmazonS3Client(bucketRegion);
            List<string> objects = new List<string>();
            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = bucketName,
                Prefix = "all_images/"
            };

            ListObjectsResponse response = s3Client.ListObjects(request);
            foreach (S3Object obj in response.S3Objects)
            {
                objects.Add(obj.Key);
            }
            return objects;
        }

        public static string GeneratePreSignedURL(double duration, string keyName)
        {
            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    Expires = DateTime.UtcNow.AddHours(duration)
                };
                urlString = s3Client.GetPreSignedURL(request1);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return urlString;
        }

    }
}
