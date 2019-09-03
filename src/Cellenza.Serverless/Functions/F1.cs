namespace Cellenza.Serverless
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Cellenza.Azure.ServiceBus;
    using Cellenza.Azure.Storage;
    using Cellenza.Serverless.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class F1
    {
        private readonly IBasicAuthService basicAuthService;
        private readonly IBlobStorageClient blobStorageClient;
        private readonly IServiceBusClient serviceBusClient;
        private readonly IConfigurationRoot configuration;
        private readonly ILogger logger;

        public F1(
            IBasicAuthService basicAuthService,
            IBlobStorageClientFactory blobStorageClientFactory,
            IServiceBusClientFactory serviceBusClientFactory,
            IConfigurationRoot configuration,
            ILoggerFactory loggerFactory)
        {
            this.basicAuthService = basicAuthService;
            this.configuration = configuration;

            this.blobStorageClient = blobStorageClientFactory.Create(
                new BlobStorageClientContext(
                    this.configuration["StorageConnectionString"],
                    this.configuration["StorageBlobContainerName"]));

            this.serviceBusClient = serviceBusClientFactory.Create(
                new ServiceBusClientContext(
                    this.configuration["ServiceBusConnectionString"],
                    this.configuration["ServiceBusQueueName"]));

            this.logger = loggerFactory.CreateLogger<F1>();
        }

        [FunctionName("F1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest httpRequest)
        {
            this.logger.LogInformation("[F1] Function is starting (HttpTrigger)");

            var authResult = this.basicAuthService.Authenticate(
                httpRequest,
                this.configuration["Basic:Authentication:Function:F1"]);

            if (!authResult.Succeeded)
            {
                this.logger.LogError(authResult.Failure.Message);

                return new UnauthorizedResult();
            }

            var httpRequestBody = await new StreamReader(httpRequest.Body).ReadToEndAsync();

            var blobName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}-{GetIdentifier(httpRequestBody)}.json";

            var blob = await this.blobStorageClient.CreateAsync(
                blobName,
                httpRequestBody,
                "application/json",
                new Dictionary<string, string> // blob metadata
                {
                    { "RemoteIpAddress", httpRequest.HttpContext.Connection.RemoteIpAddress.ToString() }
                });

            var message = new ForwardMessage { BlobUri = blob.Uri.AbsoluteUri };

            this.logger.LogInformation("[F1] Pushing message to the ServiceBus Queue");

            await this.serviceBusClient.Push(message);

            return new OkObjectResult(message);
        }

        private string GetIdentifier(string httpRequestBody)
        {
            var id = string.Empty;

            try
            {
                dynamic parser = JsonConvert.DeserializeObject(httpRequestBody);

                id = parser.id;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
            }

            return !string.IsNullOrWhiteSpace(id) ? id : "UnknownId";
        }
    }
}