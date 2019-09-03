namespace Cellenza.Serverless
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    public class F2
    {
        private readonly ILogger logger;

        public F2(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<F2>();
        }

        [FunctionName("F2")]
        public void Run(
            [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] Message message)
        {
            this.logger.LogInformation($"[F2] Function is starting (ServiceBusTrigger)");

            var messageBody = message.ReadBodyAsString();

            this.logger.LogInformation($"[F2] Processing message: {message.Label}, Content = {messageBody}");
        }
    }
}