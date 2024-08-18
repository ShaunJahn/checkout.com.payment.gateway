using Newtonsoft.Json;

public class EventData
{
    public string Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string Status { get; init; }

    [JsonIgnore]
    public string CardNumber { get; private set; }

    [JsonProperty("CardNumber")]
    public string MaskedCardNumber { get; private set; }

    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; }
    public int Amount { get; init; }
    public string Cvv { get; init; }

    public EventData(string id, DateTime timestamp, string status, string cardNumber, int expiryMonth, int expiryYear, string currency, int amount, string cvv)
    {
        Id = id;
        Timestamp = timestamp;
        Status = status;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Currency = currency;
        Amount = amount;
        Cvv = cvv;

        // Set the CardNumber and MaskedCardNumber
        CardNumber = cardNumber;
        MaskedCardNumber = cardNumber.Length > 4 ? $"**** **** **** {cardNumber[^4..]}" : cardNumber;
    }
}
