using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoWorks.Cryptsy.Entities
{
    public class MarketTrade
    {
        /// <summary>
        ///A unique ID for the trade
        /// </summary>
        [JsonProperty("tradeid")]
        public string Tradeid { get; set; }

        /// <summary>
        ///Server datetime trade occurred
        /// </summary>
        [JsonProperty("datetime")]
        public string Datetime { get; set; }

        /// <summary>
        ///The price the trade occurred at
        /// </summary>
        [JsonProperty("tradeprice")]
        public string Tradeprice { get; set; }

        /// <summary>
        ///Quantity traded
        /// </summary>
        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        /// <summary>
        ///Total value of trade (tradeprice * quantity)
        /// </summary>
        [JsonProperty("total")]
        public string Total { get; set; }

        /// <summary>
        ///The type of order which initiated this trade
        /// </summary>
        [JsonProperty("initiate_ordertype")]
        public string InitiateOrdertype { get; set; }
    }
}
