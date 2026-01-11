namespace TradeWeb.Domain;
//usage not implemented with current version, using strings intead of writing to stream will decrease performance
//will be preferable for storing data in DB or returning in lesser portions
public sealed record EnrichedTrade(
    TradeDate Date,
    string ProductName,
    string Currency,
    decimal Price);
