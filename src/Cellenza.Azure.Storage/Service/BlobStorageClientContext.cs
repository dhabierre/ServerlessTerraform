namespace Cellenza.Azure.Storage
{
    using System;

    public class BlobStorageClientContext
    {
        public BlobStorageClientContext(string connectionString, string containerName)
        {
            this.ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.ContainerName = containerName ?? throw new ArgumentNullException(nameof(containerName));

            this.ContainerName = this.ContainerName.ToLowerInvariant();
        }

        public string ConnectionString { get; }

        public string ContainerName { get; }
    }
}