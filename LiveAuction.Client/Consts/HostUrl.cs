namespace LiveAuction.Client.Consts;

public static class HostUrl
{
    // Set at startup: local API in Debug builds, current site origin in Release builds.
    public static string Current { get; set; } = string.Empty;
}
