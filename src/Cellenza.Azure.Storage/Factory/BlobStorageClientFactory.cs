namespace Cellenza.Azure.Storage
{
    public class BlobStorageClientFactory : IBlobStorageClientFactory
    {
        public IBlobStorageClient Create(BlobStorageClientContext context)
        {
            return new BlobStorageClient(context);
        }
    }
}