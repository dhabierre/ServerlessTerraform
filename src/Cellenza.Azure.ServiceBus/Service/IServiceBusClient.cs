namespace Cellenza.Azure.ServiceBus
{
    using System.Threading.Tasks;

    public interface IServiceBusClient
    {
        Task Push(object message);
    }
}