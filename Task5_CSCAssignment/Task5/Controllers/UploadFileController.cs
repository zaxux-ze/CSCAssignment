using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Task5.Controllers
{
    public class UploadFileController : ApiController
    {
        private const string bucketName = "zes3";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast1;
        private static IAmazonS3 s3Client;

        [HttpPost]
        [Route("api/uploadFileToS3")]
        public async Task<HttpResponseMessage> PostFormData()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            try
            {
                Guid guid = new Guid();
                string fileName = guid.ToString();

                var fileTransferUtility =
                    new TransferUtility(s3Client);

                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                byte[] file = await provider.Contents[0].ReadAsByteArrayAsync();

                Stream fileStream = new MemoryStream(file);

                fileTransferUtility.Upload(fileStream, bucketName, fileName);

                string objectUrl = String.Format("https://{0}.s3.{1}.amazonaws.com/{2}", bucketName, bucketRegion.SystemName, fileName);

                return Request.CreateResponse(HttpStatusCode.OK, new { url = objectUrl });
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
