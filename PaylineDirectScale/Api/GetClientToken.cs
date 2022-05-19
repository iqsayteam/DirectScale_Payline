using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Services;
using PaylineDirectScale.Model;
using System;

namespace PaylineDirectScale.Api
{
    public class GetClientToken : IApiEndpoint
    {
        private readonly ICurrencyService _currencyService;
        private readonly IAssociateService _associateService;
        private readonly IRequestParsingService _requestParsing;
        private readonly ILoggingService _logger;
        private readonly IOrderService _orderService;
        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;
        private readonly ICompanyService _companyService;

        public GetClientToken(IRequestParsingService requestParsing, ICurrencyService currencyService, IAssociateService associateService, ILoggingService loggingService, IOrderService orderService, ISettingsService settingsService, IDataService dataService, ICompanyService companyService)
        {
            _requestParsing = requestParsing ?? throw new ArgumentNullException(nameof(requestParsing));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _associateService = associateService ?? throw new ArgumentNullException(nameof(associateService));
            _logger = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        }

        public ApiDefinition GetDefinition()
        {
            return new ApiDefinition
            {
                Route = "payline/getSessionTokenForAuthEvent",
                Authentication = AuthenticationType.None
            };
        }
        public IApiResponse Post(ApiRequest request)
        {
            var paymentIn = _requestParsing.ParseBody<TokenRequest>(request);

            if (paymentIn == null)
            {
                _logger.LogInformation($"Could not parse token request. Got body: {request.Body}");
                throw new Exception("The inbound token request could not be parsed. It must include CustomerId and DiscoMerchantId.");
            }

            try
            {
                var moneyIn = Initializer.GetMoneyInInstanceForMerchantId(paymentIn.DiscoMerchantId, _currencyService, _associateService, _logger, _orderService, _settingsService, _dataService, _companyService);

                return new Ok(new { SessionToken = moneyIn.GetSessionTokenForAuthEvent(paymentIn) });
            }
            catch (Exception e)
            {
                return new Ok(new { ErrorMessage = e.Message });
            }
        }
    }
}
