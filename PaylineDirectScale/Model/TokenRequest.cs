using Newtonsoft.Json;

namespace PaylineDirectScale.Model
{
    public class TokenRequest
    {
        [JsonProperty("AssociateId")]
        public string AssociateId { get; set; }

        [JsonProperty("DiscoMerchantId")]
        public int DiscoMerchantId { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("CardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("ExpirationDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty("CVV")]
        public string CVV { get; set; }

        [JsonProperty("OneTimeAuthToken")]
        public string OneTimeAuthToken { get; set; }

        [JsonProperty("CardType")]
        public string CardType { get; set; }

    }
}
