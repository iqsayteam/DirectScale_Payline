using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Services;
using System;

namespace PaylineDirectScale.Api
{
    public class RedirectCallback : IApiEndpoint
    {
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;
        private readonly IEmailService _emailService;

        public RedirectCallback(ILoggingService loggingService,ISettingsService settingsService, IDataService dataService, IEmailService emailService)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public ApiDefinition GetDefinition()
        {
            return new ApiDefinition
            {
                Route = "payline/callback",
                Authentication = AuthenticationType.None
            };
        }
        public IApiResponse Post(ApiRequest request)
        {
            return new Ok();
        }
    }
}
