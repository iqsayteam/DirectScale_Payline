namespace PaylineDirectScale.Model
{
    public class PaylineRefundResponse
    {
        public bool Successful { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
    }
}
