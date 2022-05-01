using PaylineDirectScale.Model;

namespace PaylineDirectScale.Services.Interface
{
    public interface IPaylineService
    {
        PaylineChargeResponse ChargeAmount(PaylineChargeData data);
        // RefundResponse RefundTransaction(RefundTransaction refundData);
        //void DeletePayment(string payorId, string paymentMethodId);
        //DirectScale.Disco.Extension.PaymentMethod[] GetCustomerPaymentMethods(int payorId, string CurrencyCode);
    }
}
