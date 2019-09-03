namespace Cellenza.Azure.Storage
{
    public interface IBlobStorageClientFactory
    {
        IBlobStorageClient Create(BlobStorageClientContext context);
    }
}