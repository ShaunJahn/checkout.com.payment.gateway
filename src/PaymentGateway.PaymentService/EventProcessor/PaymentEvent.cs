using Newtonsoft.Json;

public class PaymentEvent
{
    public string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
    public string CardNumber { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
    public string Cvv { get; set; }

    [JsonConstructor]
    public PaymentEvent(string id, DateTime timestamp, string status, string cardNumber, int expiryMonth, int expiryYear, string currency, int amount)
    {
        Id = id;
        Timestamp = timestamp;
        Status = status;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Currency = currency;
        Amount = amount;
        Cvv = "***";
        CardNumber = $"**** **** **** {cardNumber[^4..]}";
    }
}
