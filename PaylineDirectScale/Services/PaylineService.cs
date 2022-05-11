using DirectScale.Disco.Extension.Services;
using PaylineDirectScale.Model;
using PaylineDirectScale.Payline.Enum;
using PaylineDirectScale.Payline.Model;
using PaylineDirectScale.Payline.Utils;
using PaylineDirectScale.Services.Interface;
using System;

namespace PaylineDirectScale.Services
{
    public class PaylineService : IPaylineService
    {
        private readonly IAssociateService _associateService;
        private readonly ILoggingService _loggingService;
        private readonly IOrderService _orderService;
        private readonly ICompanyService _companyService;

        private readonly PaylineSettings _paylineSettings;
        private static readonly string _className = typeof(PaylineService).FullName;

        public PaylineService(IAssociateService associateService, ILoggingService logger, IOrderService orderService, PaylineSettings nSettings, ICompanyService companyService)
        {
            _associateService = associateService ?? throw new ArgumentNullException(nameof(associateService));
            _loggingService = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _paylineSettings = nSettings ?? throw new ArgumentNullException(nameof(nSettings));
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));

        }

        private MerchantInfo PaylineMerchantInfo
        {
            get
            {
                return new MerchantInfo(
                    _paylineSettings.PaylineMerchantSecretKey,
                    _paylineSettings.PaylineMerchantId,
                    _paylineSettings.PaylineMerchantSiteId,
                    _paylineSettings.IsLive ? ApiConstants.LiveHost : ApiConstants.IntegrationHost, 
                     HashAlgorithmType.SHA256
                   );
            }
        }

        public PaylineChargeResponse ChargeAmount(PaylineChargeData data)
        {
            var responseMsg = new PaylineChargeResponse();
            return responseMsg;
        }

        public PaylineRefundResponse RefundTransaction(RefundTransaction refundData)
        {
            var discoResponse = new PaylineRefundResponse { Successful = false };
            //try
            //{
            //    var scRefundResp = SafeCharge.RefundTransaction(refundData.RefundData.Currency, ((decimal)refundData.RefundData.PartialAmount).ToString(), refundData.Id).GetAwaiter().GetResult();
            //    discoResponse.TransactionId = scRefundResp.TransactionId;

            //    if (scRefundResp.Status == ResponseStatus.Declined || scRefundResp.Status == ResponseStatus.Error
            //        || !scRefundResp.TransactionStatus.Equals("APPROVED", StringComparison.OrdinalIgnoreCase))
            //    {
            //        _orderService.Log(refundData.RefundData.OrderNumber, $"Refund failed. {scRefundResp.GwErrorCode}:{scRefundResp.GwErrorReason}.");
            //        discoResponse.Message = $"Error {scRefundResp.GwErrorCode}: {scRefundResp.GwErrorReason}";
            //    }
            //    else
            //    {
            //        discoResponse.Successful = true;
            //        discoResponse.Message = "Refund successful";
            //    }
            //}
            //catch (Exception e)
            //{
            //    _loggingService.LogError(e, $"{_className}.RefundTransaction - Exception thrown during refund. Amount: {refundData.RefundData.PartialAmount}, Id: {refundData.Id}", null);
            //    discoResponse.Message = $"Error: {refundData.RefundData.Currency} refund may not have succeeded. Check Nuvei/Safecharge Portal.";
            //}

            return discoResponse;
        }
    }
}
