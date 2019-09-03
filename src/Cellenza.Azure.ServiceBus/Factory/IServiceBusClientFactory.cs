namespace Cellenza.Azure.ServiceBus
{
    public interface IServiceBusClientFactory
    {
        IServiceBusClient Create(ServiceBusClientContext context);
    }
}