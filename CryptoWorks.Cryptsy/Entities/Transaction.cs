using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoWorks.Cryptsy.Entities
{
    public class Transaction
    {
        /// <summary>
        /// Name of currency account
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// timestamp	The timestamp the activity posted
        /// </summary>
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

                /// <summary>
        /// The datetime the activity posted
        /// </summary>
        [JsonProperty("datetime")]
        public string Datetime { get; set; }

        /// <summary>
        /// Server timezone
        /// </summary>
        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        /// <summary>
        /// Type of activity. (Deposit / Withdrawal)
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Address to which the deposit posted or Withdrawal was sent
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// Amount of transaction (Not including any fees)
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

                /// <summary>
        /// Fee (If any) Charged for this Transaction (Generally only on Withdrawals)
        /// </summary>
        [JsonProperty("Fee")]
        public string Fee { get; set; }

        /// <summary>
        /// Network Transaction ID (If available)
        /// </summary>
        [JsonProperty("trxid")]
        public string Trxid { get; set; }


    }
}
