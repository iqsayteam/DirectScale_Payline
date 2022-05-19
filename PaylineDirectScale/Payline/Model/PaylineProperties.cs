namespace PaylineDirectScale.Payline.Model
{
    public class PaylineProperties
    {
        public string DirectPaymentAPIUrl = "https://homologation.payline.com/V4/services/DirectPaymentAPI";
        public string WebPaymentAPIUrl = "https://homologation.payline.com/V4/services/WebPaymentAPI";
        public string ExtendedAPIUrl = "https://homologation.payline.com/V4/services/ExtendedAPI";

        public string DirectPaymentAPIUrlProd = "https://services.payline.com/V4/services/DirectPaymentAPI";
        public string WebPaymentAPIUrlProd = "https://services.payline.com/V4/services/WebPaymentAPI";
        public string ExtendedAPIUrlProd = "https://services.payline.com/V4/services/ExtendedAPI";

        public bool Production = false;
        public string accessKey = "vqno3JeQdXlgMuFBaAHY";
        public string merchantID = "71467481018933";
        public string ContractNumber = "0002432";
        public string DefaultCancelUrl = "https://office2.xelliss.com//app.html#/OrderComplete?paymentSuccessful=false";

        public string homoAccessKey = "0Zgn49ToeHP00JLROT2G";
        public string homoMerchantID = "49132203652740";
        public string homoContractNumber = "1234567";
        public string homoCancelUrl = "https://xelliss.office2.directscalestage.com/app.html#/OrderComplete?paymentSuccessful=false";

        public string proxyHost = "proxy.mycompany.com";
        public string proxyPort = "9000";
        public string proxyLogin = "";
        public string proxyPassword = "";

        public string APIVersion = "30";

        //Default properties
        public string DefaultPaymentCurrency = "978";

        public string DefaultOrderCurrency = "978";
        public string DefaultSecurityMode = "SSL";
        public string DefaultLanguage = "english";
        public string DefaultLanguageCode = "EN";
        public string DefaultPaymentAction = "100";
        public string DefaultPaymentMode = "CPT";
        public string DefaultNotificationUrl = "";
        public string DefaultReturnUrl = "";
        public string DefaultContractNumber = "CB";
        public string DefaultContractNumberList = "CB;";
        public string SecondContractNumberList = "";
        public string TermUrl = "";
        public string CustomPaymentPageCode = "";
        public string CustomPaymentTemplateUrl = "";

        public string Web2TokenKey = "";
        public string Web2TokenProdUrl = "";
        public string Web2TokenHomoUrl = "";
        public string Web2TokenContractNumber = "";
        public string Web2TokenCallbackUrl = "";
    }
}
