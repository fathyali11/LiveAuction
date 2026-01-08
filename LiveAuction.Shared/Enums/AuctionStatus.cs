using System.Text.Json.Serialization;

namespace LiveAuction.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuctionStatus
{
    Pending=0,
    Open=1,
    Closed= 2,
    Loading= 3
}
