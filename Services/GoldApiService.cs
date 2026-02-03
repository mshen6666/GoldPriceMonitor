using System.Net.Http;
using System.Net.Http.Headers;
using GoldPriceMonitor.Models;
using GoldPriceMonitor.Utils;
using Newtonsoft.Json;

namespace GoldPriceMonitor.Services;

public class GoldApiService
{
    private readonly HttpClient _client;
    private readonly string _apiToken;
    private const string BaseUrl = "https://api.itick.io";
    private const string ForexRegion = "GB";
    private const decimal UsdToCnyRate = 7.25m; // 默认汇率，可通过API更新
    private const decimal OunceToGram = 31.1035m;

    public GoldApiService(string apiToken)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _apiToken = apiToken;
    }

    public Task<MetalPrice?> GetLondonGoldAsync()
    {
        return GetForexQuoteAsync("XAUUSD");
    }

    public Task<MetalPrice?> GetLondonSilverAsync()
    {
        return GetForexQuoteAsync("XAGUSD");
    }

    public decimal ConvertToCnyPerGram(decimal usdPricePerOunce)
    {
        return usdPricePerOunce * UsdToCnyRate / OunceToGram;
    }

    public decimal CalculateCnyChange(decimal currentPrice, decimal previousPrice)
    {
        var currentCny = ConvertToCnyPerGram(currentPrice);
        var previousCny = ConvertToCnyPerGram(previousPrice);
        return currentCny - previousCny;
    }

    public decimal CalculateCnyChangePercent(decimal currentPrice, decimal previousPrice)
    {
        var currentCny = ConvertToCnyPerGram(currentPrice);
        var previousCny = ConvertToCnyPerGram(previousPrice);
        if (previousCny == 0) return 0;
        return (currentCny - previousCny) / previousCny * 100;
    }

    private async Task<MetalPrice?> GetForexQuoteAsync(string code)
    {
        try
        {
            _client.DefaultRequestHeaders.Remove("token");
            _client.DefaultRequestHeaders.Add("token", _apiToken);

            var url = $"{BaseUrl}/forex/quote?region={ForexRegion}&code={code}";
            var response = await _client.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<ItickResponse<ItickQuote>>(response);
            if (result == null || result.Code != 0 || result.Data == null)
            {
                StartupLogger.Log($"获取报价失败: {code}, code={result?.Code}, msg={result?.Msg}");
                return null;
            }

            return MapQuote(result.Data);
        }
        catch (Exception ex)
        {
            StartupLogger.Log($"获取报价异常: {code}", ex);
            return null;
        }
    }

    private static MetalPrice MapQuote(ItickQuote data)
    {
        var time = data.T > 0
            ? DateTimeOffset.FromUnixTimeMilliseconds(data.T).LocalDateTime
            : DateTime.Now;

        return new MetalPrice
        {
            Code = data.S ?? string.Empty,
            Symbol = data.S ?? string.Empty,
            Last = data.Ld,
            Chg = data.Ch,
            ChgPct = data.Chp,
            Open = data.O,
            High = data.H,
            Low = data.L,
            PreClose = data.P,
            Time = time
        };
    }
}

public class ItickResponse<T>
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("msg")]
    public string? Msg { get; set; }

    [JsonProperty("data")]
    public T? Data { get; set; }
}

public class ItickQuote
{
    [JsonProperty("s")]
    public string? S { get; set; }

    [JsonProperty("ld")]
    public decimal Ld { get; set; }

    [JsonProperty("o")]
    public decimal O { get; set; }

    [JsonProperty("p")]
    public decimal P { get; set; }

    [JsonProperty("h")]
    public decimal H { get; set; }

    [JsonProperty("l")]
    public decimal L { get; set; }

    [JsonProperty("t")]
    public long T { get; set; }

    [JsonProperty("v")]
    public decimal V { get; set; }

    [JsonProperty("tu")]
    public decimal Tu { get; set; }

    [JsonProperty("ts")]
    public int Ts { get; set; }

    [JsonProperty("ch")]
    public decimal Ch { get; set; }

    [JsonProperty("chp")]
    public decimal Chp { get; set; }
}
