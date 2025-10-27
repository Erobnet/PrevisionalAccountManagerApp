namespace PrevisionalAccountManager.Models.DataBaseEntities;

public class CurrencyModel
{
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required string Symbol { get; init; }
    public byte DecimalConvertor { get; init; }

}