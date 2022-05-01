namespace PaylineDirectScale.Payline.Model
{
    public class RefundTransaction
    {
        public string Id { get; set; }
        public RefundData RefundData { get; set; }
    }
    public class RefundData
    {
        public double Amount { get; set; }
        public string Currency { get; set; }
        public int OrderNumber { get; set; }
        public double PartialAmount { get; set; }
    }
}
