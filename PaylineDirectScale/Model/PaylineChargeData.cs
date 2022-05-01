namespace PaylineDirectScale.Model
{
    public class PaylineChargeData
    {
        public decimal Amount { get; set; }
        public string ChannelPartnerId { get; set; }
        public string Currency { get; set; }
        public int OrderNumber { get; set; }
        public string PaymentMethodToken { get; set; }
        public int PaymentId { get; internal set; }
        public string CardType { get; internal set; }
    }
}
