using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;


namespace PaylineDirectScale
{
    public class PaylineMoneyInEur : PaylineMoneyIn
    {
        public PaylineMoneyInEur(
            //bool useDSHardcodedCreds,
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            IOrderService orderService,
            ISettingsService settingsService,
            IDataService dataService,
            ICompanyService companyService
            ) : base(currencyService, associateService, loggingService, orderService, settingsService, dataService, companyService,
                 new MerchantInfo
                 {
                     Currency = "EUR",
                     DisplayName = PaylineSettings.PAYLINE_DEFAULT_SAVEDPAYMENT_DISPLAYNAME,
                     Id = PaylineSettings.PAYLINE_EUR_DISCO_MERCHANTID,
                     MerchantName = "PAYLINE (EUR)"
                 }
             )
        {
        }
    }
}
