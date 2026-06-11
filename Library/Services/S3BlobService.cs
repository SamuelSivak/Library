using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Library.Services
{
    public class S3BlobService : IBlobService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3BlobService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["MinioSettings:BucketName"] ?? "library-covers";
        }

        private async Task EnsureBucketExistsAsync()
        {
            try
            {
                var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
                if (!bucketExists)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = _bucketName,
                        UseClientRegion = false
                    };
                    await _s3Client.PutBucketAsync(putBucketRequest);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking/creating S3 bucket: {ex.Message}");
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            await EnsureBucketExistsAsync();

            var fileId = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileId,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(putRequest);
            return fileId;
        }

        public async Task<(Stream fileStream, string contentType)> DownloadFileAsync(string fileId)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileId
            };

            var response = await _s3Client.GetObjectAsync(getRequest);
            return (response.ResponseStream, response.Headers.ContentType);
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileId
                };
                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string fileId)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = fileId
                };
                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
