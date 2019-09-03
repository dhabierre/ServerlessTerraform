namespace Cellenza.Serverless
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    public class F3
    {
        private readonly ILogger logger;

        public F3(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<F3>();
        }

        [FunctionName("F3")]
        public async Task RunAsync([BlobTrigger("%StorageBlobContainerName%/{blobName}", Connection = "StorageConnectionString")] Stream blobStream,
            string blobName)
        {
            this.logger.LogInformation($"[F3] Function is starting (BlobTrigger)");

            var blobContent = await blobStream.ReadStringAsync();

            this.logger.LogInformation($"[F3] Processing blob : {blobName}, Size: {blobStream.Length} bytes, Content = {blobContent}");
        }
    }
}