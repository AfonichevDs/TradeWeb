namespace TradeWeb.Infrastructure.Helpers;
public record TradeFields(
    int DateStart, int DateLen,
    int ProductIdStart, int ProductIdLen,
    int CurrencyStart, int CurrencyLen,
    int PriceStart, int PriceLen);