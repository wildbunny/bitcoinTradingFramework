using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoWorks.Cryptsy.Entities
{
    public class Order
    {
        /// <summary>
        /// ID for this Order
        /// </summary>
        [JsonProperty("orderid")]
        public string Orderid { get; set; }

        /// <summary>
        /// The Markets ID this Order was created for
        /// </summary>
        [JsonProperty("marketid")]
        public string Marketid { get; set; }

        /// <summary>
        /// Datetime the Order was created
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Type of Order (Buy/Sell)
        /// </summary>
        [JsonProperty("ordertype")] 
        public string Ordertype { get; set; }
        /// <summary>
        /// The price per unit for this Order
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Quantity remaining for this Order
        /// </summary>
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Total value of Order (price * quantity)
        /// </summary>
        [JsonProperty("total")]
        public decimal Total { get; set; }

        /// <summary>
        /// Original Total Order Quantity
        /// </summary>
        [JsonProperty("orig_quantity")]
        public decimal OrigQuantity { get; set; }
    }
}
