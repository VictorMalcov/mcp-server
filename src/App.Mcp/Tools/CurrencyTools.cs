using App.Mcp.Contracts;
using App.Mcp.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace App.Mcp.Tools;

[McpServerToolType]
public class CurrencyTools
{
    private readonly FrankfurterClient _client;
    private readonly ILogger<CurrencyTools> _logger;

    public CurrencyTools(
        FrankfurterClient client,
        ILogger<CurrencyTools> logger)
    {
        _client = client;
        _logger = logger;
    }

    [McpServerTool(Name = "convert_currency")]
    [Description(
        "Converts an amount between two currencies using published " +
        "reference rates. Company policy converts at the rate for the date the expense was " +
        "incurred, so pass that date rather than relying on today's rate.")]
    public async Task<CurrencyConversion> ConvertCurrencyAsync(
        [Description("The amount to convert, in the source currency.")]
        decimal amount,
        [Description("Three letter ISO 4217 code to convert from, for example EUR.")]
        string from,
        [Description("Three letter ISO 4217 code to convert to, for example GBP.")]
        string to,
        [Description("The date the expense was incurred, as yyyy-MM-dd. Omit only when the date is genuinely unknown, in which case the latest rate is used.")]
        DateOnly? onDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _client.GetRateAsync(from, to, onDate, cancellationToken);
            _logger.LogInformation(
                "convert_currency called for {Amount} {From} to {To} on {OnDate}, got rate {Rate} on {RateDate}",
                amount, from, to, onDate?.ToString("yyyy-MM-dd") ?? "(latest)", rate.Rate, rate.Date.ToString("yyyy-MM-dd"));

            return new CurrencyConversion(
                   Amount: amount,
                   From: rate.Base,
                   To: rate.Quote,
                   Rate: rate.Rate,
                   RateDate: rate.Date,
                   ConvertedAmount: Math.Round(amount * rate.Rate, 2, MidpointRounding.AwayFromZero));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new McpException(
                "The exchange rate service did not respond in time. Tell the user the converted amount is unavailable rather than estimating a rate.");
        }
        catch (HttpRequestException ex)
        {
            throw new McpException(
                $"Could not reach the exchange rate service: {ex.Message}. Tell the user the converted amount is unavailable rather than estimating a rate.");
        }

    }
}
