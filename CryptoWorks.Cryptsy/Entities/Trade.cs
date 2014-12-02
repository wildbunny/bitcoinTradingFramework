using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoWorks.Cryptsy.Entities
{
    public class Trade
    {
        /// <summary>
        /// An integer identifier for this trade
        /// </summary>
        [JsonProperty("tradeid")]
        public string Tradeid { get; set; }

        /// <summary>
        /// Type of trade (Buy/Sell)
        /// </summary>
        [JsonProperty("tradetype")]
        public string Tradetype { get; set; }

        /// <summary>
        /// Server datetime trade occurred
        /// </summary>
        [JsonProperty("datetime")]
        public string Datetime { get; set; }

        /// <summary>
        /// The price the trade occurred at
        /// </summary>
        [JsonProperty("tradeprice")]
        public string Tradeprice { get; set; }

        /// <summary>
        /// Quantity traded
        /// </summary>
        [JsonProperty("quantity")]
        public string Quantity { get; set; }	

        /// <summary>
        /// Total value of trade (tradeprice * quantity) - Does not include fees
        /// </summary>
        [JsonProperty("total")]
        public decimal Total { get; set; }

        /// <summary>
        /// Fee Charged for this Trade
        /// </summary>
        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        /// <summary>
        /// The type of Order which initiated this trade
        /// </summary>
        [JsonProperty("initiate_ordertype")]
        public string InitiateOrdertype { get; set; }

        /// <summary>
        /// Original Order id this trade was executed against
        /// </summary>
        [JsonProperty("order_id")]
        public string OrderId { get; set; }
    }
}