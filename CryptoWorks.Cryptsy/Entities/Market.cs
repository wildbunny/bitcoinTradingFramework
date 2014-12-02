using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoWorks.Cryptsy.Entities
{
    public partial class Market
    {
        [JsonProperty("marketid")]
        public string Marketid { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("primaryname")]
        public string Primaryname { get; set; }

        [JsonProperty("primarycode")]
        public string Primarycode { get; set; }

        [JsonProperty("secondaryname")]
        public string Secondaryname { get; set; }

        [JsonProperty("secondarycode")]
        public string Secondarycode { get; set; }

        [JsonProperty("sellorders")]
        public List<Order> SellOrders { get; set; }

        [JsonProperty("buyorders")]
        public List<Order> BuyOrders { get; set; }

        /// <summary>
        /// Returns the highest Buy price
        /// </summary>
        public decimal HighestBuyPrice
        {
            get
            {
                if (BuyOrders != null)
                {
                    if (BuyOrders.Any())
                        return BuyOrders.FirstOrDefault().Price;
                }

                throw new Exception("Order book is empty");
            }
        }
        /// <summary>
        /// Returns the Lowest Buy price
        /// </summary>
        public decimal LowestSellPrice
        {
            get
            {
                if (SellOrders != null)
                {
                    if (SellOrders.Any())
                        return SellOrders.FirstOrDefault().Price;
                }

                throw new Exception("Order book is empty");
            }
        }

        /// <summary>
        /// The Base Price name
        /// </summary>
        public string BasePriceName
        {
            get
            {
                var parts = Label.Split(new[] { "/" }, StringSplitOptions.None);
                return parts[0];
            }
        }

        /// <summary>
        /// The quote price name
        /// </summary>
        public string PriceQuoteName
        {
            get
            {
                var parts = Label.Split(new[] { "/" }, StringSplitOptions.None);
                return parts[1];
            }
        }
    }
}