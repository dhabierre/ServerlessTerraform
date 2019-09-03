namespace Cellenza.Azure.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;

    internal class BlobStorageClient : IBlobStorageClient
    {
        private readonly CloudBlobContainer blobContainer;

        public BlobStorageClient(BlobStorageClientContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var storageAccount = CloudStorageAccount.Parse(context.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            this.blobContainer = blobClient.GetContainerReference(context.ContainerName);
        }

        public async Task<CloudBlockBlob> CreateAsync(string blobName, byte[] data, string contentType, IDictionary<string, string> metadata = null)
        {
            // should be created by IAC
            //await this.blobContainer.CreateIfNotExistsAsync(
            //    BlobContainerPublicAccessType.Off,
            //    new BlobRequestOptions(),
            //    new OperationContext());

            var blob = this.blobContainer.GetBlockBlobReference(blobName);

            blob.Properties.ContentType = contentType;

            if (metadata != null)
            {
                foreach (var meta in metadata)
                {
                    blob.Metadata.Add(meta.Key, meta.Value);
                }
            }

            await blob.UploadFromByteArrayAsync(data, 0, data.Length);

            return blob;
        }

        public async Task<CloudBlockBlob> CreateAsync(string blobName, string data, string contentType, IDictionary<string, string> metadata = null)
        {
            var buffer = Encoding.UTF8.GetBytes(data);

            return await this.CreateAsync(blobName, buffer, contentType, metadata);
        }

        public async Task<string> ReadAsStringAsync(Uri blobUri)
        {
            var blob = this.blobContainer.ServiceClient.GetBlobReferenceFromServer(new StorageUri(blobUri));

            using (var ms = new MemoryStream())
            {
                blob.DownloadToStream(ms);

                using (var reader = new StreamReader(ms))
                {
                    ms.Position = 0;

                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}