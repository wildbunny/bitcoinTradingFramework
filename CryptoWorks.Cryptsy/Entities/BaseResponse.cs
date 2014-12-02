using Newtonsoft.Json;

namespace CryptoWorks.Cryptsy.Entities
{
    public class BaseResponse
    {
        /// <summary>
        /// Was the call successful?
        /// </summary>
        [JsonProperty(PropertyName = "success")]
        public string Success { get; set; }

        /// <summary>
        /// Text description of any errors
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}