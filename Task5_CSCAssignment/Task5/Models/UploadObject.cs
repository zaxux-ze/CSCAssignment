using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Task5.Models
{
    public class UploadObject
    {
        private const string bucketName = "zes3";
        private const string keyName = "all_images";
        private const string filePath = @"E:\ZiEn_Stuff\SP\CSC\Assignment\Task5_CSCAssignment\Task5\searchTalentStart\images\";

        public void UploadFile()
        {
            //string[] files = Directory.GetFiles(filePath);
            //var client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
            //foreach (string path in files)
            //{
            try
            {
                TransferUtility directoryTransferUtility =
                    new TransferUtility(new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1));

                //// 1. Upload a directory.
                //directoryTransferUtility.UploadDirectory(filePath,
                //                                         bucketName);
                //Console.WriteLine("Upload statement 1 completed");

                //// 2. Upload only the .txt files from a directory. 
                ////    Also, search recursively. 
                //directoryTransferUtility.UploadDirectory(
                //                               filePath,
                //                               bucketName,
                //                               "*.jpg",
                //                               SearchOption.AllDirectories);
                //Console.WriteLine("Upload statement 2 completed");

                // 3. Same as 2 and some optional configuration 
                //    Search recursively for .txt files to upload).
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