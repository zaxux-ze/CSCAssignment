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
using System.Web;
using System.Web.Http;

namespace Task5.Models
{
    public class UploadObject
    {
        private const string bucketName = "zes3";
        private const string keyName = "all_images";
        private const string filePath = @"E:\ZiEn_Stuff\SP\CSC\Assignment\Task5_CSCAssignment\Task5\searchTalentStart\images\";

        public void UploadFile()
        {
            try
            {
                TransferUtility directoryTransferUtility =
                    new TransferUtility(new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1));

                TransferUtilityUploadDirectoryRequest request =
                    new TransferUtilityUploadDirectoryRequest
                    {
                        BucketName = bucketName,
                        Directory = filePath,
                        SearchOption = SearchOption.AllDirectories,
                        SearchPattern = "*.jpg",
                        KeyPrefix = keyName 
                    };

                directoryTransferUtility.UploadDirectory(request);
                //PutObjectRequest putRequest = new PutObjectRequest
                //{
                //    BucketName = bucketName,
                //    Key = keyName,
                //    FilePath = filePath,
                //    ContentType = "text/plain"
                //};

                //PutObjectResponse response = client.PutObject(putRequest);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
            //}

        }
    }
}