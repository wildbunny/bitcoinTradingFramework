using Newtonsoft.Json;

namespace CryptoWorks.Cryptsy.Entities
{
    public class OrderResponse : BaseResponse
    {
        [JsonProperty("orderid")]
        public string Orderid { get; set; }

        [JsonProperty("moreinfo")]
        public string Moreinfo { get; set; }
    }
}