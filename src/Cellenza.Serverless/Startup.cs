[assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(typeof(Cellenza.Serverless.Startup))]

namespace Cellenza.Serverless
{
    using Cellenza.Azure.ServiceBus;
    using Cellenza.Azure.Storage;
    using Cellenza.Serverless.Services;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup : FunctionsStartup
    {
        private IConfigurationRoot configuration;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            this.BuildConfiguration(builder);

            builder.Services.AddScoped<IBasicAuthService, BasicAuthService>();
            builder.Services.AddScoped<IBlobStorageClientFactory, BlobStorageClientFactory>();
            builder.Services.AddScoped<IServiceBusClientFactory, ServiceBusClientFactory>();
        }

        private void BuildConfiguration(IFunctionsHostBuilder builder)
        {
            var settingsBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            var settings = settingsBuilder.Build();

            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    new AzureServiceTokenProvider().KeyVaultTokenCallback));

            settingsBuilder.AddAzureKeyVault(
                $"https://{settings["KeyVaultName"]}.vault.azure.net/",
                keyVaultClient,
                new DefaultKeyVaultSecretManager());

            this.configuration = settingsBuilder.Build();

            builder.Services.AddSingleton(this.configuration);
        }
    }
}