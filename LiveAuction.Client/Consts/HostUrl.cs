namespace LiveAuction.Client.Consts;

public static class HostUrl
{
    public static readonly string Development = "https://localhost:7233";
    public static readonly string Production = "https://liveauction.runasp.net";

    public static string Current => Production;
}
