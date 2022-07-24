using Dapper;
using DirectScale.Disco.Extension.Services;
using PaylineDirectScale.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace PaylineDirectScale.Repositories
{
    public class PaylineDataRepository
    {
        const double SETTINGS_REFRESH_THRESHHOLD = 90.0;
        private readonly IDataService _dataService;
        private readonly ILoggingService _logger;
        private readonly ISettingsService _settingsService;

        private static Dictionary<int, PaylineSettings> SettingStore;
        private static string BaseCallbackURL;

        public PaylineDataRepository(IDataService dataService, ILoggingService logger, ISettingsService settingsService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            if (SettingStore == null) SettingStore = new Dictionary<int, PaylineSettings>();
        }

        public PaylineSettings GetSettings(int discoMerchId)
        {
            // Check for the populated settings and check their age.
            // If it's older than X seconds, re-fetch.
            // This is my attempt to ensure it's fresh across all instances.
            if (SettingStore.ContainsKey(discoMerchId))
            {
                double age = SettingStore[discoMerchId].GetSettingsAgeInSeconds();
                if (age > SETTINGS_REFRESH_THRESHHOLD) SettingStore.Remove(discoMerchId);
            }

            if (!SettingStore.ContainsKey(discoMerchId))
            {
                var isLive = _settingsService.ExtensionContext().EnvironmentType == DirectScale.Disco.Extension.EnvironmentType.Live;
                var newSettings = new PaylineSettings(discoMerchId);
                newSettings = LoadPaylineSettingsFromDB(newSettings, discoMerchId, isLive);
                newSettings.BaseCallbackUrl = GetBaseCallbackUrl(isLive);

                SettingStore.Add(discoMerchId, newSettings);
            }
            return SettingStore[discoMerchId];
        }

        private PaylineSettings LoadPaylineSettingsFromDB(PaylineSettings settings, int discoMerchantId, bool isLive)
        {
            string settingGroupName = "";

            switch (discoMerchantId)
            {
                case PaylineSettings.PAYLINE_EUR_DISCO_MERCHANTID:
                    settingGroupName = "PAYLINEEUR";
                    break;
            }

            IEnumerable<PaylineExtensionSetting> settingList = null;

            try
            {
                using (var dbConnection = new SqlConnection(_dataService.ConnectionString.ConnectionString))
                {
                    var query = $"SELECT SettingGroup, SettingKey, SettingValue FROM Client.Settings WHERE SettingGroup = '{ settingGroupName }' AND (IsLive = { (isLive ? "1" : "0 OR IsLive IS NULL") })";
                    settingList = dbConnection.Query<PaylineExtensionSetting>(query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"The Payline settings do not include a database table, or a SQL exception occurred. {ex.Message}");
            }

            // If nothing is in the table, or if the table doesn't exist, just scrap it.
            if (settingList != null && settingList.Any())
            {
                //_logger.LogInformation("Payline: Database settings found. Updating settings accordingly.");
                PopulatePaylineKeyValuesFromResults(settingList, settings);
            }

            return settings;
        }

        public void PopulatePaylineKeyValuesFromResults(IEnumerable<PaylineExtensionSetting> settingList, PaylineSettings settings)
        {
            settings.UseDatabaseSettings = "1TRUE".Contains(settingList.FirstOrDefault(x => x.SettingKey == "PaylineUseDBSettings").SettingValue?.ToUpper());
            settings.UseDirectScaleHardCodedCreds = !settings.UseDatabaseSettings; // Until we check to see if that's what the user wants in the DB settings :) 
            settings.HasLoadedDBValues = true;

            if (settings.UseDatabaseSettings) // If the client does NOT want to use these settings, then the ones passed in at Client Extension instantiation will be used. 
            {
                //_logger.LogInformation("Payline: Using database settings. Clearing all passed-in values.");
                settings.ClearSettings(); // We're going to get settings from the DB. Clear out the ones passed in.
                foreach (var setting in settingList)
                {
                    switch (setting.SettingKey)
                    {
                        case "PaylineEnvironment":
                            settings.PaylineEnvironment = setting.SettingValue;
                            break;
                        case "PaylineUseDirectScaleSandbox":
                            settings.UseDirectScaleHardCodedCreds = "1TRUE".Contains(setting.SettingValue?.ToUpper());
                            break;
                        case "PaylineMerchantId":
                            settings.PaylineMerchantId = setting.SettingValue;
                            break;
                        case "PaylineMerchantSiteId":
                            settings.PaylineMerchantSiteId = setting.SettingValue;
                            break;
                        case "PaylineMerchantSecretKey":
                            settings.PaylineMerchantSecretKey = setting.SettingValue;
                            break;
                        case "PaylineIFrameDimensions":
                            settings.PaylineIFrameDimensions = setting.SettingValue;
                            break;
                        case "PaylineAcceptedCardTypes":
                            settings.PaylineAcceptedCardTypes = setting.SettingValue;
                            break;
                        case "PaylineSavedPaymentDisplayName":
                            settings.SavedPaymentDisplayName = setting.SettingValue;
                            break;
                        case "PaylineReCaptchaSiteKey":
                            settings.ReCaptchaSiteKey = setting.SettingValue;
                            break;
                        case "PaylineReCaptchaSecretKey":
                            settings.ReCaptchaSecretKey = setting.SettingValue;
                            break;
                        case "PaylineContractNumber":
                            settings.PaylineContractNumber = setting.SettingValue;
                            break;
                        case "PaylineVersion":
                            settings.PaylineVersion = setting.SettingValue;
                            break;
                        case "PaylineDirectPaymentAPIUrl":
                            settings.PaylineDirectPaymentAPIUrl = setting.SettingValue;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private string GetBaseCallbackUrl(bool isLive)
        {
            // The Callback URL will never change on this particular environment. We can store it statically.
            if (BaseCallbackURL == null)
            {
                BaseCallbackURL =
                    $"https://{_settingsService.ExtensionContext().ClientId}.corpadmin.directscale{(isLive ? string.Empty : "stage")}.com";
            }
            return BaseCallbackURL;
        }
    }
}
