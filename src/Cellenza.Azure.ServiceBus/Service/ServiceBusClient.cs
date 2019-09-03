namespace Cellenza.Azure.ServiceBus
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;

    internal class ServiceBusClient : IServiceBusClient
    {
        private readonly QueueClient queueClient;

        public ServiceBusClient(ServiceBusClientContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this.queueClient = new QueueClient(context.ConnectionString, context.QueueName);
        }

        public async Task Push(object message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var json = JsonConvert.SerializeObject(message);
            var data = Encoding.UTF8.GetBytes(json);

            await this.queueClient.SendAsync(new Message(data));
        }
    }
}