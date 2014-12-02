using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoWorks.Cryptsy.Entities
{
    public class Info
    {
        /// <summary>
        /// Array of currencies and the balances availalbe for each
        /// </summary>
        [JsonProperty(PropertyName = "balances_available")]
        public dynamic BalancesAvailable { get; set; }

        /// <summary>
        /// Array of currencies and the amounts currently on hold for open orders
        /// </summary>
        [JsonProperty(PropertyName = "balances_hold")]
        public dynamic BalancesHold { get; set; }

        /// <summary>
        /// Current server timestamp
        /// </summary>
        [JsonProperty(PropertyName = "servertimestamp")]
        public string Servertimestamp { get; set; }

        /// <summary>
        /// Current timezone for the server
        /// </summary>
        [JsonProperty(PropertyName = "servertimezone")]
        public string Servertimezone { get; set; }

        /// <summary>
        /// Current date/time on the server
        /// </summary>
        [JsonProperty(PropertyName = "serverdatetime")]
        public string Serverdatetime { get; set; }

        /// <summary>
        /// Count of open orders on your account
        /// </summary>
        [JsonProperty(PropertyName = "openordercount")]
        public string Openordercount { get; set; }
    }
}
