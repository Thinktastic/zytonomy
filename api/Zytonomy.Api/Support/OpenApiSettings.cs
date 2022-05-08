
/// <summary>
/// See: https://github.com/Azure/azure-functions-openapi-extension/blob/main/samples/Microsoft.Azure.WebJobs.Extensions.OpenApi.FunctionApp.V3IoC/Configurations/AppSettings.cs
/// </summary>
namespace Zytonomy.Api.Support;
public class AppSettings : OpenApiAppSettingsBase
{
    public AppSettings() : base()
    {
        this.OpenApi = this.Config.Get<OpenApiSettings>("OpenApi");
    }

    public virtual OpenApiSettings OpenApi { get; set; }
}

public class OpenApiSettings
{
    public virtual string ApiKey { get; set; }
}
