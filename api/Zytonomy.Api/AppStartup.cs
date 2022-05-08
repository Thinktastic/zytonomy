[assembly: FunctionsStartup(typeof(Zytonomy.Api.AppStartup))]

namespace Zytonomy.Api;

/// <summary>
///     Startup class used to initialize the dependency injection.
/// </summary>
/// <remarks>
///     See: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
/// </remarks>
public class AppStartup : FunctionsStartup
{
    /// <summary>
    ///     Initializes the dependency injection container.
    /// </summary>
    /// <param name="builder">The handle to the host builder.</param>
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<AppSettings>();

        // Registration for identity service which manages metadata for verifying tokens signed by Azure AD B2C
        builder.Services.AddSingleton<AzIdentityService>(AzIdentityService.Initialize());

        // Registration for identity service which manages verification of local tokens.
        // TODO:

        // Registration for Q&A Maker Client.
        // TODO: This needs to be migrated to the new client when it's out of beta
        builder.Services.AddSingleton<QnAMakerClient>(new QnAMakerClient(
            new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("QnA_ApiKey"))
        ) {
            Endpoint = Environment.GetEnvironmentVariable("QnA_Endpoint")
        });

        // Registration for access to the cloud storage account
        CloudStorageAccount cloudStorage = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("ContentSourceStorage"));
        builder.Services.AddSingleton<CloudStorageAccount>(cloudStorage);

        // Registration for main blob client which needs the shared credential key to generate SAS keys.
        builder.Services.AddSingleton<BlobContainerClient>(
            BlobContainerFactory.CreateDefault()
        );

        // Registrations for domain data access.
        builder.Services.AddSingleton(new CosmosClient(
                Environment.GetEnvironmentVariable("Cosmos_Endpoint"),
                Environment.GetEnvironmentVariable("Cosmos_AuthKey"),
                new CosmosClientOptions {
                    Serializer = new SystemTextJsonCosmosSerializer(),
                    AllowBulkExecution = true
                }));

        builder.Services.AddSingleton<CosmosGateway>();

        // Add repositories as singletons.
        // TODO: Add reflection to peform the same.
        builder.Services.AddSingleton<WorkspaceRepository>();
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<MessageRepository>();
        builder.Services.AddSingleton<NoteRepository>();
        builder.Services.AddSingleton<InvitationRepository>();

        // Add mutators
        builder.Services.AddSingleton<UserWorkspaceMutator>();
    }
}
