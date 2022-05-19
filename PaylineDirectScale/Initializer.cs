using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using PaylineDirectScale.Api;
using PaylineDirectScale.Services;
using PaylineDirectScale.Services.Interface;
using System;

namespace PaylineDirectScale
{
    public static class Initializer
    {
        public static void UsePayline(this IServiceCollection services)
        {
            services.AddSingleton<IPaylineService, PaylineService>();

            services.AddSingleton<IMoneyInMerchant, PaylineMoneyInEur>();
            services.AddSingleton<IApiEndpoint, GetClientToken>();
        }

        public static PaylineMoneyIn GetMoneyInInstanceForMerchantId(int mId, ICurrencyService _currencyService, IAssociateService _associateService, ILoggingService _logger, IOrderService _orderService, ISettingsService _settingsService, IDataService _dataService, ICompanyService _companyService)
        {
            switch (mId)
            {
                case PaylineSettings.PAYLINE_EUR_DISCO_MERCHANTID:
                    return new PaylineMoneyInEur(_currencyService, _associateService, _logger, _orderService, _settingsService, _dataService, _companyService) ?? throw new Exception("The payment processor is incorrectly configured.");
                 default:
                    _logger.LogError(new Exception(), $"Payline Error: Cannot get token. Merchant ID {mId} is not valid.");
                    throw new Exception($"Payline Error: Cannot get token. Merchant ID {mId} is not valid.");
            }
        }
    }
}
