using System;
using System.Collections.Generic;

namespace PaylineDirectScale
{
    public class PaylineSettings
    {
        public const int PAYLINE_EUR_DISCO_MERCHANTID = 9861;
        public const string PAYLINE_DEFAULT_IFRAMEDIMENSIONS = "550x450";
        public const string PAYLINE_DEFAULT_ACCEPTEDCARDTYPES = "visa,mastercard"; // possibilities: visa, mastercard, amex, diners, discover, jcb, dankort, unionPay
        public const string PAYLINE_VALID_ACCEPTEDCARDTYPES = "visa,mastercard";
        public const string PAYLINE_DEFAULT_SAVEDPAYMENT_DISPLAYNAME = "Payline Credit Card";
        public readonly HashSet<string> PAYLINE_ACCEPTED_LOCALES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "en_US","en_UK","de_DE","zh_CN","iw_IL","fr_FR","nl_NL","in_ID","it_IT","ja_JP","ko_KR","lt_LT","es_ES","en_CA","en_AU","ru_RU","ar_AA","pt_BR","sv_SE","tr_TR","sl_SI","da_DK","ro_RO","bg_BG","pl_PL","hu_HU","vi_VL" };


        private string _paylineEnvironment;
        private string _paylineMerchantId;
        private string _paylineMerchantSiteId;
        private string _paylineMerchantSecretKey;
        private string _paylineIFrameDimensions;
        private string _paylineAcceptedCardTypes;
        private string _paylineSavedPaymentDisplayName;
        private string _paylineContractNumber;
        private string _paylineDirectPaymentAPIUrl;
        private string _paylineVersion;

        private DateTime InstantiationTime { get; set; }

        public PaylineSettings(int discoMerchId, bool useDSHardcodedCreds = true, bool enableDBSettings = true)
        {
            InstantiationTime = DateTime.Now;
            DiscoMerchantId = discoMerchId;
            UseDirectScaleHardCodedCreds = useDSHardcodedCreds;
            EnableDBSettings = enableDBSettings;
        }

        /// <summary>
        /// This is the partner ID, hard-coded to DirectScale's value.
        /// </summary>
        public int DiscoMerchantId { get; set; }
        public bool EnableDBSettings { get; set; } // The program will read the DB for settings
        public bool HasLoadedDBValues { get; set; } = false;
        public bool IsLive => "production".Equals(PaylineEnvironment, StringComparison.CurrentCultureIgnoreCase);
        public bool UseDatabaseSettings { get; set; } = false; // The program will use the settings it finds in the DB
        public bool UseDirectScaleHardCodedCreds { get; set; } = false;
        public string ReCaptchaSiteKey { get; internal set; }
        public string ReCaptchaSecretKey { get; internal set; }

        public double GetSettingsAgeInSeconds()
        {
            return (DateTime.Now - InstantiationTime).TotalSeconds;
        }

        public void ClearSettings()
        {
            UseDirectScaleHardCodedCreds = false;
            _paylineEnvironment = string.Empty;
            _paylineMerchantId = string.Empty;
            _paylineMerchantSiteId = string.Empty;
            _paylineIFrameDimensions = null;
            _paylineContractNumber = string.Empty;
            _paylineDirectPaymentAPIUrl = string.Empty;
        }

        public string BaseCallbackUrl { get; set; }
        public string PaylineEnvironment
        {
            get
            {
                var paylineEnvironment = UseDirectScaleHardCodedCreds ? "sandbox" : _paylineEnvironment;
                if (string.IsNullOrEmpty(paylineEnvironment) || !"production sandbox".Contains(paylineEnvironment))
                {
                    throw new Exception($"The Payline Environment setting has not been set, or has been set incorrectly. Correct values are 'production' or 'sandbox'. Actual value: {paylineEnvironment}");
                }
                return paylineEnvironment;
            }
            set => _paylineEnvironment = value;
        }
        // This is the DirectScale general merchant ID. Site IDs will be specific to clients.
        public string PaylineMerchantId
        {
            get => UseDirectScaleHardCodedCreds ? "49132203652740" : _paylineMerchantId;
            set => _paylineMerchantId = value;
        }

        // This is the identifier of individual, client-specific merchant accounts. There may be 
        // many of these per client relationship with Payline. This is generally set up by currency accepted.
        public string PaylineMerchantSiteId
        {
            get
            {
                return UseDirectScaleHardCodedCreds ? "212850" : _paylineMerchantSiteId;
            }
            set => _paylineMerchantSiteId = value;
        }
        public string PaylineContractNumber
        {
            get
            {
                return UseDirectScaleHardCodedCreds ? "1234567" : _paylineContractNumber;
            }
            set => _paylineContractNumber = value;
        }

        // This is the API key specific to each client's site.  
        public string PaylineMerchantSecretKey
        {
            get => UseDirectScaleHardCodedCreds ? "0Zgn49ToeHP00JLROT2G" : _paylineMerchantSecretKey;
            set => _paylineMerchantSecretKey = value;
        }

        public string PaylineDirectPaymentAPIUrl
        {
            get => UseDirectScaleHardCodedCreds ? "https://homologation.payline.com/V4/services/DirectPaymentAPI" : _paylineDirectPaymentAPIUrl;
            set => _paylineDirectPaymentAPIUrl = value;
        }

        public string PaylineVersion
        {
            get => UseDirectScaleHardCodedCreds ? "30" : _paylineVersion;
            set => _paylineVersion = value;
        }

        public int IFrameWidth => GetIFrameDimensions("Width");
        public int IFrameHeight => GetIFrameDimensions("Height");

        public string PaylineAcceptedCardTypes
        {
            get { return string.IsNullOrEmpty(_paylineAcceptedCardTypes) ? PAYLINE_DEFAULT_ACCEPTEDCARDTYPES : _paylineAcceptedCardTypes; }
            set { _paylineAcceptedCardTypes = value; }
        }

        public string PaylineIFrameDimensions
        {
            get { return string.IsNullOrEmpty(_paylineIFrameDimensions) ? PAYLINE_DEFAULT_IFRAMEDIMENSIONS : _paylineIFrameDimensions; }
            set { _paylineIFrameDimensions = value; }
        }

        public string SavedPaymentDisplayName
        {
            get { return string.IsNullOrEmpty(_paylineSavedPaymentDisplayName) ? PAYLINE_DEFAULT_SAVEDPAYMENT_DISPLAYNAME : _paylineSavedPaymentDisplayName; }
            set { _paylineSavedPaymentDisplayName = value; }
        }

        private int GetIFrameDimensions(string dimensionToGet)
        {
            var dimensionParts = PaylineIFrameDimensions.Split('x');
            if (dimensionParts.Length != 2)
            {
                throw new Exception($"IFrame Dimension Setting '{PaylineIFrameDimensions}' must be formatted WxH");
            }

            try
            {
                switch (dimensionToGet)
                {
                    case "Width":
                        return Convert.ToInt32(dimensionParts[0]);
                    case "Height":
                        return Convert.ToInt32(dimensionParts[1]);
                    default:
                        return 0;
                }
            }
            catch (Exception)
            {
                throw new Exception($"IFrame Dimension Setting 'Iframe Dimensions' must be formatted WxH. Current setting: {PaylineIFrameDimensions}");
            }
        }
    }
}
