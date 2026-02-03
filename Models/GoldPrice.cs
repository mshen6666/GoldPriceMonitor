namespace GoldPriceMonitor.Models;

public class MetalPrice
{
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Last { get; set; }
    public decimal Chg { get; set; }
    public decimal ChgPct { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Open { get; set; }
    public decimal PreClose { get; set; }
    public DateTime Time { get; set; }
}

public class PricePoint
{
    public DateTime Time { get; set; }
    public decimal Price { get; set; }
}

public class DisplayPrice
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public bool IsPositive { get; set; }
    public string FormattedPrice { get; set; } = string.Empty;
    public string FormattedChange { get; set; } = string.Empty;
}
