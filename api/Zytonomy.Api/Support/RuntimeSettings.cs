namespace Zytonomy.Api.Support;

/// <summary>
/// Convenience class for accessing runtime settings.
/// </summary>
public static class RuntimeSettings
{
    /// <summary>
    /// The ID of the knowledge base.  This may need to change in the future to accommodate multiple
    /// KBs.
    /// </summary>
    public static string KbId => Environment.GetEnvironmentVariable("Kb_Id");
}
