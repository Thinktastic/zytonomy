namespace Zytonomy.Api.Endpoints.Workflow;

/// <summary>
/// Durable Function orchestration which controls the flow of KB provisioning for
/// a workspace.  This is a multi-step process which involves: 1) initiating the
/// KB creation and specifying the initial set of documents, 2) monitoring the
/// provisioning, 3) downloading the contents, 4) amending the metadata for every
/// item.
/// </summary>
public class WorkspaceProvisioningFlow
{

    public WorkspaceProvisioningFlow()
    {

    }

    /// <summary>
    /// Entry point which starts this workflow.
    /// </summary>
    [FunctionName("WorkspaceProvisioningFlow")]
    public async Task Start(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        log.LogInformation(">>> WorkspaceProvisioningFlow is starting...");

        Workspace workspace = context.GetInput<Workspace>();

        string provisioningOpId = await context.CallActivityAsync<string>("KbManagementActivities_UpdateKbSources", workspace);

        log.LogInformation(">>> WorkspaceProvisioningFlow starting timer");

        for (int i = 0; i < 20; i++)
        {
            log.LogInformation(">>> Staring timer...");

            // Same function as Thread.Sleep(); 3 second wait.
            await context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(3), CancellationToken.None);

            string operationState = await context.CallActivityAsync<string>(
                "KbManagementActivities_Monitor", provisioningOpId);

            if (OperationStateType.Failed == operationState)
            {
                throw new Exception("Failed to provision the KB.");
            }

            if (OperationStateType.Succeeded == operationState)
            {
                break; // Exit the loop.
            }
        }

        string updateOpId = await context.CallActivityAsync<string>("KbManagementActivities_UpdateMetadata", workspace);

        log.LogInformation(">>> WorkspaceProvisioningFlow starting timer");

        for (int i = 0; i < 20; i++)
        {
            log.LogInformation(">>> Staring timer...");

            // Same function as Thread.Sleep(); 3 second wait.
            await context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(3), CancellationToken.None);

            string operationState = await context.CallActivityAsync<string>(
                "KbManagementActivities_Monitor", updateOpId);

            if (OperationStateType.Failed == operationState)
            {
                throw new Exception("Failed to update the KB.");
            }

            if (OperationStateType.Succeeded == operationState)
            {
                break; // Exit the loop.
            }
        }

        await context.CallActivityAsync<string>("KbManagementActivities_Publish", workspace);
    }
}
