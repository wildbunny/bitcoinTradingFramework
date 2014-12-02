using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CryptoWorks.Cryptsy.Entities;
using Newtonsoft.Json.Linq;

namespace CryptoWorks.Cryptsy
{
    public class CryptsyApi
    {
        /// <summary>
        /// Cryptsy public key
        /// </summary>
        public static string PublicKey { get; set; }

        /// <summary>
        /// Cryptsy private key
        /// </summary>
        public static string PrivateKey { get; set; }

        /// <summary>
        /// Creates an order
        /// </summary>
        /// <param name="marketId">The market to place the order</param>
        /// <param name="orderType">Buy or Sell option</param>
        /// <param name="quantity">The quantity to buy/Sell</param>
        /// <param name="price">The offer price to Buy/Sell</param>
        /// <returns>The order response value from the api</returns>
        public static OrderResponse CreateOrder(string marketId, OrderType orderType, decimal quantity, decimal price)
        {
            var pairs = new Dictionary<string, string>
                {
                    {"marketid", marketId},
                    {"ordertype", orderType.ToString()},
                    {"quantity", quantity.ToString()},
                    {"price", price.ToString()}
                };

            var json = Base("createorder", pairs);

            var o = JObject.Parse(json);
            var result = o.ToObject<OrderResponse>();


            return result;

        }

        /// <summary>
        /// Gets all trades 
        /// </summary>
        /// <param name="marketId">Optional marketid to return trades from single market only</param>
        /// <returns>List of all trades</returns>
        public static List<Trade> GetTrades(string marketId = "0", int maxRetryCount = 3)
        {
            var pairs = new Dictionary<string, string>
                {
                    {"marketid", marketId}
                };

            var json = marketId == "0" ? Base("allmytrades", maxRetryCount: maxRetryCount) : Base("mytrades", pairs, maxRetryCount);

            var jObject = JObject.Parse(json);
            var value = jObject.GetValue("return");

            if (!value.HasValues)
            {
                return new List<Trade>();
            }

            var trades = value.ToObject<List<Trade>>();

            return trades;
        }

        /// <summary>
        ///  Array of Deposits and Withdrawals on your account 
        /// </summary>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns></returns>
        public static List<Transaction> GetMyTransactions(int maxRetryCount = 3)
        {
            var json = Base("mytransactions", maxRetryCount: maxRetryCount);

            var jObject = JObject.Parse(json);
            var ret = jObject.GetValue("return");

            if (ret == null)
            {
                return new List<Transaction>();
            }

            if (!ret.HasValues)
            {
                return new List<Transaction>();
            }

            var result = ret.ToObject<List<Transaction>>();

            return result;
        }

        /// <summary>
        ///   Array of last 1000 Trades for this Market, in Date Decending Order  
        /// </summary>
        /// <param name="marketId">the marketid to query</param>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns>Array of last 1000 Trades for this Market, in Date Decending Order  </returns>
        public static List<MarketTrade> GetMarketTrades(string marketId, int maxRetryCount = 3)
        {
            var pairs = new Dictionary<string, string>
                {
                    {"marketid", marketId}
                };

            var json = Base("markettrades", pairs, maxRetryCount);

            var jObject = JObject.Parse(json);
            var ret = jObject.GetValue("return");

            if (ret == null)
            {
                return new List<MarketTrade>();
            }

            if (!ret.HasValues)
            {
                return new List<MarketTrade>();
            }

            var result = ret.ToObject<List<MarketTrade>>();

            return result;
        }


        /// <summary>
        /// Lists all orders in all markets
        /// </summary>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns></returns>
        public static List<Order> GetAllMyOrders(int maxRetryCount = 3)
        {
            var json = Base("allmyorders",maxRetryCount:maxRetryCount);

            var jObject = JObject.Parse(json);
            var ret = jObject.GetValue("return");

            if (ret == null)
            {
                return new List<Order>();
            }

            if (!ret.HasValues)
            {
                return new List<Order>();
            }

            var result = ret.ToObject<List<Order>>();

            return result;
        }

        /// <summary>
        /// Cancells all orders in all markets
        /// </summary>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns></returns>
        public static List<string> CancellAllOrders(int maxRetryCount = 3)
        {
            var json = Base("cancelallorders");

            var jObject = JObject.Parse(json);
            var ret = jObject.GetValue("return");

            if (ret == null)
            {
                return new List<string>();
            }

            if (!ret.HasValues)
            {
                return new List<string>();
            }

            var result = ret.ToObject<List<string>>();

            return result;
        }

        /// <summary>
        /// Get info on your cryptsy account
        /// </summary>
        /// <returns>The info object</returns>
        public static Info GetInfo()
        {
            var json = Base("getinfo");

            var jObject = JObject.Parse(json);
            var value = jObject.GetValue("return");

            if (!value.HasValues)
            {
                return new Info();
            }

            var info = value.ToObject<Info>();

            return info;

        }

        /// <summary>
        /// Returns the market data 
        /// </summary>
        /// <param name="marketId">Market id</param>
        /// <param name="maxRetryCount">Number of retrys, defaults to 3</param>
        /// <returns>A single market</returns>
        public static Market GetSingleMarket(string marketId = "", int maxRetryCount = 3)
        {
            var market = GetMarkets(marketId,maxRetryCount).FirstOrDefault();
            return market;
        }

        /// <summary>
        /// Returns the market data 
        /// </summary>
        /// <param name="marketId">Optional parameter for single market</param>
        /// <param name="maxRetryCount">Number of retrys, defaults to 3</param>
        /// <returns>A list of all markets</returns>
        public static List<Market> GetMarkets(string marketId = "", int maxRetryCount = 3)
        {
            while (true)
            {
                try
                {
                    var markets = GetMarketRetry(marketId);
                    return markets;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }
        }

        public static List<Market> GetMarketRetry(string marketId = "")
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", "Nobody");

            var url = String.Empty;

            if (marketId == String.Empty)
            {
                url = "http://pubapi.cryptsy.com/api.php?method=marketdatav2";
            }
            else
            {
                url = string.Format("http://pubapi.cryptsy.com/api.php?method=singlemarketdata&marketid={0}", marketId);
            }

            var json = client.DownloadString(new Uri(url));
            var jObject = JObject.Parse(json);
            var value = jObject.GetValue("return");

            return value.First.First.Select(x => x.First.ToObject<Market>()).ToList();
        }


        private static string Base(string method, Dictionary<string, string> paramList = null, int maxRetryCount = 3)
        {
            if (string.IsNullOrEmpty(PublicKey) || string.IsNullOrEmpty(PrivateKey))
            {
                throw new Exception("Please set public and private key using the static properties");
            }

            var request = (HttpWebRequest)WebRequest.Create("https://www.Cryptsy.com/api");
            var postData = String.Format("method={0}&nonce={1}", method,Environment.TickCount);
            request.ServicePoint.Expect100Continue = false;

            if (paramList != null)
            {
                postData = paramList.Aggregate(postData, (current, pair) => current + String.Format("&{0}={1}", pair.Key, pair.Value));
            }

            var hmAcSha = new HMACSHA512(Encoding.ASCII.GetBytes(PrivateKey));
            var messagebyte = Encoding.ASCII.GetBytes(postData);
            var hashmessage = hmAcSha.ComputeHash(messagebyte);
            var sign = BitConverter.ToString(hashmessage);
            sign = sign.Replace("-", "");

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = messagebyte.Length;
            request.Method = "POST";
            request.Headers.Add("Key", PublicKey);
            request.Headers.Add("Sign", sign.ToLower());
            
            try
            {
                var stream = request.GetRequestStream();
                stream.Write(messagebyte, 0, messagebyte.Length);
                stream.Close();
                var response = request.GetResponse();
                var postreqreader = new StreamReader(response.GetResponseStream());
                var json = postreqreader.ReadToEnd();
                return json;
            }
            catch (Exception exception)
            {
                if (--maxRetryCount == 0)
                    throw exception;
            }

            return null;
        }
    }
}
