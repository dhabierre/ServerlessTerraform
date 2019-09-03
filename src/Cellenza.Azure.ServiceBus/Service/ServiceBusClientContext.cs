namespace Cellenza.Azure.ServiceBus
{
    using System;

    public class ServiceBusClientContext
    {
        public ServiceBusClientContext(string connectionString, string queueName)
        {
            this.ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        public string ConnectionString { get; }

        public string QueueName { get; }
    }
}