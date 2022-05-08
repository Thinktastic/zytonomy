namespace Zytonomy.Api.Support;

/// <summary>
/// See: https://github.com/Azure/azure-functions-openapi-extension/blob/main/samples/Microsoft.Azure.WebJobs.Extensions.OpenApi.FunctionApp.V3IoC/Configurations/OpenApiConfigurationOptions.cs
/// </summary>
public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
    {
        Version = "3.0.0",
        Title = "OpenAPI Sample on Azure Functions (IoC)",
        Description = "A sample API that runs on Azure Functions (IoC) 3.x using OpenAPI specification.",
        TermsOfService = new Uri("https://github.com/Azure/azure-functions-openapi-extension"),
        Contact = new OpenApiContact()
        {
            Name = "Contoso",
            Email = "azfunc-openapi@contoso.com",
            Url = new Uri("https://github.com/Azure/azure-functions-openapi-extension/issues"),
        },
        License = new OpenApiLicense()
        {
            Name = "MIT",
            Url = new Uri("http://opensource.org/licenses/MIT"),
        }
    };
}
