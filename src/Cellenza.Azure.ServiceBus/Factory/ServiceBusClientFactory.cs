namespace Cellenza.Azure.ServiceBus
{
    public class ServiceBusClientFactory : IServiceBusClientFactory
    {
        public IServiceBusClient Create(ServiceBusClientContext context)
        {
            return new ServiceBusClient(context);
        }
    }
}