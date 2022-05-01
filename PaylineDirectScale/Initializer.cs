using DirectScale.Disco.Extension;
using Microsoft.Extensions.DependencyInjection;
using PaylineDirectScale.Services;
using PaylineDirectScale.Services.Interface;

namespace PaylineDirectScale
{
    public static class Initializer
    {
        public static void UsePayline(this IServiceCollection services)
        {
            services.AddSingleton<IPaylineService, PaylineService>();

            services.AddSingleton<IMoneyInMerchant, PaylineMoneyInEur>();
        }
    }
}
