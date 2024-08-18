using Newtonsoft.Json;

public class PaymentRequest
{
    [JsonProperty("card_number")]
    public string CardNumber { get; }

    [JsonProperty("expiry_date")]
    public string ExpiryDate { get; }

    [JsonProperty("currency")]
    public string Currency { get; }

    [JsonProperty("amount")]
    public int Amount { get; }

    [JsonProperty("cvv")]
    public string Cvv { get; }

    public PaymentRequest(string cardNumber, int expiryMonth, int expiryYear, string currency, int amount, string cvv)
    {
        CardNumber = cardNumber;
        ExpiryDate = $"{expiryMonth:D2}/{expiryYear}";
        Currency = currency;
        Amount = amount;
        Cvv = cvv;
    }
}
