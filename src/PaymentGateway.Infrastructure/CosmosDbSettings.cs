namespace PaymentGateway.Infrastructure
{
    public class CosmosDbSettings
    {
        public required string Account { get; set; }
        public required string Key { get; set; }
        public required string DatabaseId { get; set; }
        public required string ContainerId { get; set; }
    }
}
