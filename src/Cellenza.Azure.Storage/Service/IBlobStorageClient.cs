namespace Cellenza.Azure.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Blob;

    public interface IBlobStorageClient
    {
        Task<CloudBlockBlob> CreateAsync(string blobName, byte[] data, string contentType, IDictionary<string, string> metadata = null);

        Task<CloudBlockBlob> CreateAsync(string blobName, string data, string contentType, IDictionary<string, string> metadata = null);

        Task<string> ReadAsStringAsync(Uri blobUri);
    }
}