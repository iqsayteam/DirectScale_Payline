using Dapper;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;
using Newtonsoft.Json;
using PaylineDirectScale.Model;
using PaylineDirectScale.Payline.Model;
using PaylineDirectScale.Repositories;
using PaylineDirectScale.Services;
using SDKPaylineDotNet.WebPaymentAPI;
using System;
using System.Linq;
using System.Text;

namespace PaylineDirectScale
{
    public abstract class PaylineMoneyIn : SavedPaymentMoneyInMerchant
    {
        private PaylineService _paylineService;
        private readonly PaylineSettings _paylineSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IDataService _dataService;
        private readonly IAssociateService _associateService;
        private readonly ILoggingService _loggingService;
        private readonly IOrderService _orderService;
        private readonly ISettingsService _settingsService;
        private readonly ICompanyService _companyService;

        public PaylineMoneyIn(ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            IOrderService orderService,
            ISettingsService settingsService,
            IDataService dataService,
            ICompanyService companyService,
            DirectScale.Disco.Extension.MerchantInfo merchInfo
            ) : base(merchInfo, paymentFormWidth: 400, paymentFormHeight: 500)
        {
            _associateService = associateService;
            _currencyService = currencyService;
            _dataService = dataService;
            _loggingService = loggingService;
            _orderService = orderService;
            _settingsService = settingsService;
            _companyService = companyService;

            PaylineDataRepository data = new PaylineDataRepository(_dataService, _loggingService, _settingsService);

            _paylineSettings = data.GetSettings(merchInfo.Id);
            _paylineService = new PaylineService(_associateService, _loggingService, _orderService, _paylineSettings, _companyService);

            base.PaymentFormWidth = _paylineSettings.IFrameWidth;
            base.PaymentFormHeight = _paylineSettings.IFrameHeight;
            merchInfo.DisplayName = _paylineSettings.SavedPaymentDisplayName;
        }

        public override string GenerateOneTimeAuthToken(string payerId, int associateId, string languageCode, string countryCode)
        {
            return _paylineService.GetHashCode().ToString();
            //LogMessage($"{DateTime.UtcNow} - PayerId:{payerId}, associateId:{associateId},languageCode:{languageCode},countrycode:{countryCode}", "GenerateOneTimeAuthToken");
            // var associate = _associateService.GetAssociate(associateId);
            // var isTempAssociate = associate.AssociateBaseType == 0;
            // return GetToken(associate, isTempAssociate);
        }

        public override SavePaymentForm GetSavePaymentForm(string payerId, int associateId, string languageCode, string countryCode, string oneTimeAuthToken)
        {
            LogMessage($"{DateTime.UtcNow} - PayerId:{payerId}, associateId:{associateId},languageCode:{languageCode},countrycode:{countryCode},oneTimeAuthToken:{oneTimeAuthToken}", "GetSavePaymentForm");

            var associate = _associateService.GetAssociate(associateId);
            var isTempAssociate = associate.AssociateBaseType == 0;
            string head = $@"
                         <script src='https://ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.min.js'></script>
                         <script src='http://igorescobar.github.io/jQuery-Mask-Plugin/js/jquery.mask.min.js'></script>

 <script>
    $(document).ready(() => {{
                              show(document.getElementsByClassName('toggle-content')[0]);
                              $('#cardNumber').mask('0000 0000 0000 0000');
                           }});
    function addSlashes (element) {{
           let ele = document.getElementById(element.id);
           ele = ele.value.split('/').join('');   
           if(ele.length < 4 && ele.length > 0){{
               let finalVal = ele.match(/.{{1,2}}/g).join('/');
           
               document.getElementById(element.id).value = finalVal;
           }}
    }}
    function SavePaylinePayment(payload) {{
        var outData = {{}};
        outData.token = payload.userPaymentOptionId + ""|"" + payload.transactionId + ""|"" + window.cardType;
        outData.type = window.cardType; 
        outData.expireMonth = payload.ccExpMonth;
        outData.expireYear = payload.ccExpYear;
        outData.last4 = payload.last4Digits;
    
        DS_SavePaymentMethod(outData);
    }}    
</script>
{GetStyles()}";

            string body = $@"<!-- Body of HTML page here, to render iFrame -->

     <div class='webSDK-placeholder'>
     <div class='container'>
         
         <!-- Credit Card Payment Form - START -->
         <div class='webSDK-container toggle-content container'>
             <div class='row'>
                 <div class='col-xs-12 col-md-4 col-md-offset-4'>
                     <div class='panel panel-default'>
                         <div class='panel-heading'>
                             <div class='row'>
                                 <div class='inlineimage'>  
                                     { GetAcceptedCardIcons() }
                                 </div>
                             </div>
                         </div>
                         <div class='panel-body'>
                             <form role='form'>
                                 <div class='row'>
                                     <div class='col-xs-12'>
                                         <div class='form-group'> <label>CARD NUMBER</label> <input type='text' id='cardNumber' class='form-control'> </div>
                                     </div>
                                 </div>
                                 <div class='row'>
                                     <div class='col-xs-6'>
                                         <div class='form-group'> <label>CARD EXPIRY</label> <input type='text' id='cardExpiry' class='form-control' placeholder='MM/YY' onkeyup='addSlashes(this)'> </div>
                                     </div>
                                     <div class='col-xs-6'>
                                         <div class='form-group'> <label>CVV</label> <input type='text' id='cardCVV' class='form-control' placeholder='CVV'> </div>
                                     </div>
                                 </div>
                                 <div class='row'>
                                     <div class='col-xs-6'>
                                         <div class='form-group'> <label>CARD OWNER</label> <input type='text' id='cardHolderName' class='form-control' placeholder='Card Owner Name' value='{(isTempAssociate ? "" : associate.Name)}'> </div>
                                     </div>
                                     <div class='col-xs-6'>
                                         <div class='form-group'> <label>EMAIL</label> <input type='text' id='cardHolderEmail' class='form-control' placeholder='Card Owner Email' value='{(isTempAssociate ? "" : associate.EmailAddress)}'> </div>
                                     </div>
                                 </div>
                                 <div class='row'>
                                     <div class='col-xs-12'>
                                         <div id='scard-errors' class='alert alert-warning' style='display: none;'></div>
                                     </div>
                                 </div>
                             </form>
                         </div>
                         <div class='panel-footer'>
                             <div class='row'>
                                 <div class='col-xs-12'> <button id='pay-button' class=' btn btn-success btn-lg btn-block'>Save Payment</button> </div>
                             </div>
                         </div>
                     </div>
                 </div>
             </div>
         </div>
     </div>
     </div>
     </div>";
            return new SavePaymentForm
            {
                Body = body,
                Head = head
            };
        }

        public override void DeletePayment(string payerId, string paymentMethodId)
        {
            // Right now, this doesn't do anything on the Nuvei side.
            // _nuveiService.DeletePayment(payerId, paymentMethodId);
        }

        public override PaymentResponse ChargePayment(string payerId, NewPayment payment, int orderNumber)
        {
            LogMessage($"{DateTime.UtcNow} - PayerId:{payerId}, orderNumber:{orderNumber},payment:{JsonConvert.SerializeObject(payment)}", "ChargePayment");
            // Incoming data validation
            if (string.IsNullOrWhiteSpace(payment.PaymentMethodId)) throw new ArgumentNullException(payment.PaymentMethodId);

            // When it was saved, the payment token includes PayPal or APM in those conditions, along with transaction ID, etc.
            if (payment.PaymentMethodId.ToUpperInvariant().Contains("PAYPAL"))
            {
                payment.CardType = "PayPal";
            }

            if (payment.Amount < 1) throw new ArgumentException("Amount cannot be less than 1", nameof(payment.Amount));

            int payerIdIntValue = GetPayerIdAsInt(payerId);
            if (_associateService.GetAssociate(payerIdIntValue) == null) throw new ArgumentNullException($"PayorID of {payerId} is invalid.");

            var res = new PaymentResponse { Status = PaymentStatus.Rejected };

            try
            {
                PaylineChargeData data = new PaylineChargeData
                {
                    Amount = Convert.ToDecimal(payment.Amount),
                    Currency = payment.CurrencyCode.ToUpper(),
                    PaymentMethodToken = payment.PaymentMethodId,
                    OrderNumber = orderNumber,
                    CardType = payment.CardType
                };

                PaylineChargeResponse chargeResponse = _paylineService.ChargeAmount(data);

                res.Response = chargeResponse.ProcessorResponseMessage;
                res.ResponseId = chargeResponse.ProcessorResponseCode;
                res.TransactionNumber = chargeResponse.TransactionId;
                res.AuthorizationCode = chargeResponse.PaymentIdentifier;
                res.Status = chargeResponse.IsSuccessful ? PaymentStatus.Accepted : PaymentStatus.Rejected;
            }
            catch (Exception e)
            {
                _loggingService.LogError(e, $"Exception thrown when sending or processing Nuvei payment response for order {orderNumber}.");
            }

            res.Amount = payment.Amount;
            res.OrderNumber = orderNumber;
            res.PaymentType = PaylinePaymentTypes.Charge;
            res.Currency = payment?.CurrencyCode?.ToUpper();
            res.Merchant = MerchantInfo.Id;
            res.Redirect = false;
            return res;
        }

        public override PaymentResponse RefundPayment(string payerId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string referenceNumber, string transactionNumber, string authorizationCode)
        {
            paymentAmount = _currencyService.Round(paymentAmount, currencyCode);
            refundAmount = _currencyService.Round(refundAmount, currencyCode);
            var refundTrans = new RefundTransaction
            {
                Id = transactionNumber,
                RefundData = new RefundData
                {
                    Amount = paymentAmount,
                    OrderNumber = orderNumber,
                    PartialAmount = refundAmount,
                    Currency = currencyCode.ToUpper()
                }
            };

            var response = _paylineService.RefundTransaction(refundTrans);

            _loggingService.LogInformation($"Processed refund for order {orderNumber}. TransactionId: {transactionNumber}, Amount: {refundTrans.RefundData.Amount}, Returned status: {response}.");

            return new PaymentResponse
            {
                Amount = refundAmount,
                AuthorizationCode = authorizationCode,
                Currency = currencyCode.ToUpper(),
                Merchant = MerchantInfo.Id,
                OrderNumber = orderNumber,
                TransactionNumber = response.TransactionId,
                ResponseId = "0",
                PaymentType = PaylinePaymentTypes.Credit,
                Response = response.Message,
                Status = response.Successful ? PaymentStatus.Accepted : PaymentStatus.Rejected
            };
        }


        private int GetPayerIdAsInt(string payerId)
        {
            int payerIdAsInt;
            if (payerId.All(char.IsDigit))
            {
                payerIdAsInt = Convert.ToInt32(payerId);
            }
            else
            {
                _loggingService.LogWarning($"Failed to convert payerId '{payerId}' to int. Expecting integer Disco ID.");
                throw new Exception($"Could not fetch payment information for the customer provided. (ID: {payerId})");
            }
            return payerIdAsInt;
        }

        private string GetToken(Associate associate, bool isTempAssociate)
        {
            LogMessage($"{DateTime.UtcNow} - associate:{associate.AssociateId}, isTempAssociate:{isTempAssociate}", "GetToken");
            WebPaymentAPI myWebPaymentAPI = SDKPaylineDotNet.PaymentApiFactory.GetWebPaymentAPIClient();

            try
            {
                //var result = myWebPaymentAPI.doWebPayment("30", ExtractPayment(),
                //                              "https://localhost:44346/Home/About",
                //                              "https://localhost:44346/Home/Contact",
                //                              ExtractDummyOrder(),
                //                              "https://localhost:44346/Home/About",
                //                              new string[] { "0002432" },
                //                              new string[] { },
                //                              null,
                //                              "FR",
                //                              "",
                //                              new buyer() { email = $"{(isTempAssociate ? "" : associate.EmailAddress)}" },
                //                               null, "SSL", null, null, null, null, null, null, null,
                //                               new threeDSInfo() { challengeInd = "01", threeDSReqPriorAuthMethod = "01", threeDSReqPriorAuthTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                //                               null, null,
                //                              out string token,
                //                              out string redirectURL,
                //                              out string stepCode,
                //                              out string reqCode,
                //                              out string method);
                //LogMessage($"{DateTime.UtcNow} - associate:{associate.AssociateId}, isTempAssociate:{isTempAssociate}, token: {token}", "GetToken");
                //return token;
                return string.Empty;
            }
            catch (Exception ex)
            {
                LogMessage($"{DateTime.UtcNow} - {ex.Message}", "GetToken");
            }


            return string.Empty;
        }

        private payment ExtractPayment()
        {
            return new payment
            {
                amount = "1",
                currency = "978",
                action = "101",
                mode = "CPT",
                contractNumber = "0002432"
            };
        }

        private order ExtractDummyOrder()
        {
            return new order
            {
                @ref = "1",
                origin = "Xelliss",
                currency = "978",
                amount = "1",
                date = DateTime.Now.ToString("dd/MM/yyyy hh:mm")
            };
        }

        public string GetStyles()
        {
            return $@"<style>
    .container {{
      display: flex;
      flex-wrap: wrap;
      order: 2;
      justify-content: left;
    }}

    div#card-field-placeholder {{
        height: 35px;
        width: 100%;
        min-width: 400px;
    }}

    #result {{
      margin-top: 1rem;
      background-color: #3a4453;
      color: #fff;
      font-size: 12px;
    }}

    .item {{
      /* flex: 0 48%; */
      height: 100%;
      margin-bottom: 2%; /* (100-32*3)/2 */
      padding-right: 15px;
    }}
    .item100 {{
      flex: 0 100%;
      margin-bottom: 2%; /* (100-32*3)/2 */
      justify-content: center;
    }}
    input#cardHolderName {{
        width: 100%;
    }}

    .toggle-content.is-visible {{
	    display: block;
    }}

    .webSDK-placeholder {{
      height: 20rem;
    }}

     input {{
	     background: #ffffff;
	     border-radius: 3px;
	     border: 1px solid #d3dce6;
	     color: #4a5568;
	     display: block;
	     outline: none;
	     padding: 5px 12px;
	     width: 360px;
	     font-family: Nunito Sans, sans-serif;
    }}
     input:placeholder-shown {{
	     border: 1px solid #d3dce6;
    }}
     input:hover {{
	     border: 1px solid #95aac1;
    }}
     input:focus {{
	     border: 1px solid #00becf;
    }}
     #card-field-placeholder, #card-number, #card-expiry, #card-cvc {{
	     background: #ffffff;
	     border-radius: 3px;
	     border: 1px solid #d3dce6;
	     color: #4a5568;
	     display: block;
	     margin-top: 5px;
	     outline: none;
	     padding: 7px 12px;
	     width: 360px;
    }}
     #card_number_three_fields {{
	     margin-top: 5px;
	     padding: 7px 12px;
	     width: 470px;
    }}
     .pay-button {{
	     background-color: #00becf;
	     border-radius: 3px;
	     border: 1px solid #00A6B5;
	     color: #fff;
	     font-size: 14px;
	     font-weight: 400;
	     height: 36px;
	     line-height: 20px;
	     margin-top: 10px;	 
	     margin-bottom: 10px;
	     width: 140px;
    }}
    .pay-button:hover {{
      color: #fff;
      background-color: #0f93a1;
    }}
     .validation-error {{
	     color: #FF3F5A;
	     font-size: 12px;
	     height: 16px;
	     line-height: 16px;
	     visibility: hidden;
    }}

     label {{
	     color: #4a5568;
	     font-size: 14px;
	     font-weight: 600;
    }}

     #card-field-placeholder.sfc-focus, #card-number.focused, #card-expiry.focused, #card-cvc.focused  {{
	     border: 1px solid #00becf;
    }}

    .container {{
        padding-left: 0;
        padding-right: 0;
    }}

     #card-field-placeholder.sfc-focus, #card-number:hover, #card-expiry:hover, #card-cvc:hover {{
	     border: 1px solid #95aac1;
    }}
 
    button, input, optgroup, select, textarea {{
      margin: 0;
      font-family: inherit;
      font-size: inherit;
      line-height: inherit;
    }}

    body {{
      font-family: Nunito Sans, Roboto,sans-serif;
      font-size: 14px;
      font-weight: 400;
      color: #4a5568;
      line-height: 1.8;
    }}


     .sfcModal-header {{
	     height: 1.5rem;
    }}
     .sfcModal-dialog {{
	     margin: 55px auto;
	     max-width: 492px;
	     position: relative;
	     width: auto;
    }}
     .sfcModal-content {{
	     background-clip: padding-box;
	     background-color: #ffffff;
	     border: 1px solid #dfdfdf;
	     outline: 0;
	     position: relative;
    }}
    .sfcModal-close {{
 	     border: 0;
 	     color: #2c2a2a;
 	     cursor: pointer;
 	     font-size: .9rem;
 	     padding: 0;
 	     position: absolute;
 	     right: 0.5rem;
 	     top: 0.4rem;
     }}
     .sfcIcon--close:before {{
 	     content: '\2716';
     }}
    div#scard-errors {{
        color: red;
    }}
.inlineimage {{
    max - width: 470px;
    margin-right: 8px;
    margin-left: 10px
}}

.images {{
    display: inline-block;
    max-width: 98%;
    height: auto;
    width: 10%;
    margin: 1%;
    left: 20px;
    text-align: center
}}
</style>";
        }

        public string GetAcceptedCardIcons()
        {
            var sb = new StringBuilder();
            var acceptedCardList = _paylineSettings.PaylineAcceptedCardTypes.ToLower();
            var cards = acceptedCardList.Split(',');
            foreach (var card in cards)
            {
                if (PaylineSettings.PAYLINE_VALID_ACCEPTEDCARDTYPES.Contains(card))
                {
                    sb.Append($"<img class='img-responsive images' src='https://cdn.safecharge.com/safecharge_resources/v1/websdk/img/rounded/{card.Trim()}.svg'>");
                }
            }
            return sb.ToString();
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
