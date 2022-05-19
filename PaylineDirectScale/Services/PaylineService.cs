using Dapper;
using DirectScale.Disco.Extension.Services;
using Newtonsoft.Json;
using PaylineDirectScale.Model;
using PaylineDirectScale.Payline.Enum;
using PaylineDirectScale.Payline.Model;
using PaylineDirectScale.Payline.Utils;
using PaylineDirectScale.Services.Interface;
using ServiceReference1;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PaylineDirectScale.Services
{
    public class PaylineService : IPaylineService
    {
        private readonly IAssociateService _associateService;
        private readonly ILoggingService _loggingService;
        private readonly IOrderService _orderService;
        private readonly ICompanyService _companyService;
        private readonly IDataService _dataService;
        private readonly PaylineSettings _paylineSettings;
        private static readonly string _className = typeof(PaylineService).FullName;

        public PaylineService(IAssociateService associateService, ILoggingService logger, IOrderService orderService, IDataService dataService, PaylineSettings nSettings, ICompanyService companyService)
        {
            _associateService = associateService ?? throw new ArgumentNullException(nameof(associateService));
            _loggingService = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _paylineSettings = nSettings ?? throw new ArgumentNullException(nameof(nSettings));
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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
            LogMessage($"{DateTime.UtcNow} - PaylineChargeData:{JsonConvert.SerializeObject(data)}", "ChargeAmount");
            var responseMsg = new PaylineChargeResponse();
            try
            {
                // Gather some DS data on the order and associate
                DirectScale.Disco.Extension.Order orderInfo = _orderService.GetOrderByOrderNumber(data.OrderNumber);

                LogMessage($"{DateTime.UtcNow} - orderInfo:{JsonConvert.SerializeObject(orderInfo)}", "ChargeAmountOrderInfo");

                string paymentIdentifier = $"{ data.OrderNumber }+{ orderInfo.Payments.Count }";
              
                var cardToken = data.PaymentMethodToken;

                var paymentDetail = Task.Run<doImmediateWalletPaymentResponse>(async () => await CreateWalletRequest(orderInfo, cardToken, data.AssociateId, data.Amount));
                var paymentResponse = paymentDetail.Result;
                
                LogMessage($"{DateTime.UtcNow} - paymentResponse:{JsonConvert.SerializeObject(paymentResponse)}", "paymentResponse");
                
                if (paymentResponse.result.shortMessage == "ACCEPTED")
                {
                    var isSuccessful = paymentResponse.result.shortMessage.Equals("ACCEPTED", StringComparison.OrdinalIgnoreCase);
                    responseMsg = new PaylineChargeResponse
                    {
                        IsSuccessful = isSuccessful,
                        PaymentIdentifier = paymentIdentifier,
                        TransactionId = paymentResponse.transaction.id,
                        ProcessorResponseCode = isSuccessful ? "0" : "400",
                        ProcessorResponseMessage = isSuccessful ? paymentResponse.result.shortMessage : "Payline error",
                    };
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"{_className}.ChargeAmount - Exception thrown in Charge Amount. Amount: {data.Amount},Order: {data.OrderNumber}", ex);
            }
            return responseMsg ?? new PaylineChargeResponse { IsSuccessful = false, ProcessorResponseCode = "E", ProcessorResponseMessage = "A non-processor error occurred." };
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

        private async Task<doImmediateWalletPaymentResponse> CreateWalletRequest(DirectScale.Disco.Extension.Order orderInfo, string walletId,int associateId, decimal amount)
        {
            var associate = _associateService.GetAssociate(associateId);
            return await DoImmediateWalletPayment(new doImmediateWalletPaymentRequest()
            {
                threeDSInfo = new threeDSInfo() { challengeInd = "01", threeDSReqPriorAuthMethod = "01", threeDSReqPriorAuthTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                walletId = walletId,
                payment = ExtractPayment(orderInfo, _paylineSettings.PaylineContractNumber),
                buyer = new buyer()
                {
                    email = associate.EmailAddress,
                    firstName = associate.DisplayFirstName,
                    lastName = associate.DisplayLastName,
                    walletId = walletId,
                    billingAddress = new address
                    {
                        name = associate.Name,
                        street1 = associate.Address?.AddressLine1,
                        street2 = associate.Address?.AddressLine2,
                        cityName = associate.Address?.City,
                        state = associate.Address?.State,
                        country = associate.Address?.CountryCode,
                        zipCode = associate.Address?.PostalCode,
                    },
                    shippingAdress = new address
                    {
                        name = associate.Name,
                        street1 = associate.ShipAddress?.AddressLine1,
                        street2 = associate.ShipAddress?.AddressLine2,
                        cityName = associate.ShipAddress?.City,
                        state = associate.ShipAddress?.State,
                        country = associate.ShipAddress?.CountryCode,
                        zipCode = associate.ShipAddress?.PostalCode,
                        
                    }
                },
                order = ExtractOrderInfo(orderInfo)
            });
        }

        public async Task<doImmediateWalletPaymentResponse> DoImmediateWalletPayment(doImmediateWalletPaymentRequest immediateWalletPaymentRequest)
        {
            var factory = GetDirectFactory();
            var serviceProxy = PaymentApiFactory.GetDirectServiceProxy(factory);
            var opContext = new OperationContext((IClientChannel)serviceProxy);
            var prevOpContext = OperationContext.Current; // Optional if there's no way this might already be set
            OperationContext.Current = opContext;
            var result = new doImmediateWalletPaymentResponse();
            try
            {
                result = await serviceProxy.doImmediateWalletPaymentAsync(immediateWalletPaymentRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                PaymentApiFactory.DisposeServiceProxy(factory, serviceProxy, prevOpContext);
            }
            return result;
        }

        public ChannelFactory<DirectPaymentAPI> GetDirectFactory()
        {
            var factory = new ChannelFactory<DirectPaymentAPI>(PaymentApiFactory.CreateBindingAndInitializeClient(), new EndpointAddress(new Uri(_paylineSettings.PaylineDirectPaymentAPIUrl)));
            factory.Credentials.UserName.UserName = _paylineSettings.PaylineMerchantId;
            factory.Credentials.UserName.Password = _paylineSettings.PaylineMerchantSecretKey;
            return factory;
        }

        private static order ExtractOrderInfo(DirectScale.Disco.Extension.Order order)
        {
            var amount = order.Totals[0].Total * 100;
            var paylineOrder = new order
            {
                @ref = order.OrderNumber.ToString(),
                amount = amount.ToString(),
                currency = "978",
                date = order.OrderDate.ToString("dd/MM/yyyy hh:mm"),
                origin = "1"
            };

            int index = 0;
            paylineOrder.details = new orderDetail[order.LineItems.Count];
            foreach (var item in order.LineItems)
            {
                orderDetail paylineOrderDetail = new()
                {
                    quantity = item.Qty.ToString(),
                    @ref = item.ItemId.ToString(),
                    price = item.Cost.ToString(),
                    brand = item.ProductName.ToString(),
                    category = item.SKU.ToString()
                };
                paylineOrder.details[index] = paylineOrderDetail;
                index++;
            }

            return paylineOrder;
        }

        private static payment ExtractPayment(DirectScale.Disco.Extension.Order order,string paylineContractNumber)
        {
            var amount = order.Totals[0].Total * 100;
            return new payment()
            {
                amount = amount.ToString(),
                currency = "978",
                action = "101", //123
                mode = "CPT", //REC
                contractNumber = paylineContractNumber
            };
        }
        private void LogMessage(string msg, string invoked)
        {
            var logMessage = new PaylineLogMessage(DateTime.Now.ToLocalTime(), msg, "debug", invoked);

            try
            {
                using (var dbConnection =
                    new System.Data.SqlClient.SqlConnection(_dataService.ClientConnectionString.ConnectionString))
                {
                    string sql =
                        "INSERT INTO Client.logs([datetime], [message], [lvl], [invoked]) VALUES (@DateTimeStamp, @Message, @LevelString, @Invoked); SELECT CAST (SCOPE_IDENTITY() as int)";
                    int result = dbConnection.Execute(sql, logMessage);
                }
            }
            catch (Exception e)
            {
                var logMsg = new PaylineLogMessage(DateTime.Now.ToLocalTime(), msg, "critical", invoked);

                using (var dbConnection =
                    new System.Data.SqlClient.SqlConnection(_dataService.ClientConnectionString.ConnectionString))
                {
                    string sql =
                        "INSERT INTO Client.logs([datetime], [message], [lvl]) VALUES (@DateTimeStamp, @Message, @LevelString); SELECT CAST (SCOPE_IDENTITY() as int)";
                    int result = dbConnection.Execute(sql, logMsg);
                }
            }
        }
    }
}
