using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace HuobiApi
{
    public class Huobi : MarketBase
    {
        public const decimal kCent = 0.01M;
        public const decimal kMinAmount = 0.001M;
        private const string kApiRoot = "https://api.huobi.com/apiv2.php";
        public const string kGet = "GET";
        public const string kPost = "POST";
        public const int kRetryCount = 10;

        /// <summary>
        /// </summary>
        /// <param name="huobiTime"></param>
        public Huobi(string accessKey, string secretKey, double sleepAfterCallSeconds = 1.5) : base(accessKey, secretKey, sleepAfterCallSeconds)
        {
        }

        public override T2 Send<T2>(string method, params object[] args)
        {
            // add some more args for authentication
            uint seconds = UnixTime.GetFromDateTime(DateTime.UtcNow) + (uint)m_timeOffset.TotalSeconds;

            var moreArgs = new Dictionary<string, string>
            {
                {"created", seconds.ToString()},
                {"access_key", m_accessKey},
                {"method", method},
                {"secret_key", m_secretKey}
            };

            for (int i = 0; i < args.Length / 2; i++)
            {
                moreArgs[args[i * 2 + 0].ToString()] = HttpUtility.UrlEncode(args[i * 2 + 1].ToString());
            }

            List<KeyValuePair<string, string>> sortedByKey = moreArgs.OrderBy(kvp => kvp.Key).ToList();

            string hashArgs = RestHelpers.BuildPostArgs(sortedByKey);
            string paramsHash = MD5(hashArgs);

            sortedByKey.Add(new KeyValuePair<string, string>("sign", paramsHash));

            // remove secret key from query params
            sortedByKey.Remove(sortedByKey[sortedByKey.Count - 2]);

            string query = RestHelpers.BuildPostArgs(sortedByKey);

            var request = new HuobiSyncWebRequest<T2>(kApiRoot, null, kPost, query, 10, kRetryCount);
            request.ContentType = "application/x-www-form-urlencoded";

            bool done = false;

            T2 obj = default(T2);
            DateTime now = DateTime.UtcNow;

            if (m_sleepAfterCallSeconds > 0)
            {
                if (m_lastCalled.ContainsKey(method))
                {
                    TimeSpan deltaLast = now - m_lastCalled[method];

                    int sleepMillis = (int)(m_sleepAfterCallSeconds * 1000) - (int)deltaLast.TotalMilliseconds;

                    if (sleepMillis > 0)
                    {
                        Console.WriteLine("Sleeping for " + sleepMillis + " method " + method);
                        Thread.Sleep(sleepMillis);
                    }
                }
            }

            while (!done)
            {
                try
                {
                    obj = request.Send();
                    done = true;
                }
                catch (HuobiException e)
                {
                    Console.WriteLine("Huobi error: " + e.m_error.code);
                    if (e.m_error.code != 71)
                    {
                        throw;
                    }
                }
            }

            m_lastCalled[method] = now;

            return obj;
        }

        /// <summary>
        /// </summary>
        /// <param name="amount"></param>
        public override void ValidateAmount(decimal amount)
        {
            decimal trucated = Numerical.TruncateDecimal(amount, 4);
            WildLog.Assert(trucated == amount, "Amount should have 4 decimal places max");

            WildLog.Assert(amount >= kMinAmount, "Minimum tradable amount is " + kMinAmount);
        }

        /// <summary>
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        public override List<HuobiOrder> GetOpenOrders(HuobiMarket coinType)
        {
            return Send<List<HuobiOrder>>("get_orders", "coin_type", (int)coinType);
        }

        public override HuobiAccountInfo GetAccountInfo()
        {
            return Send<HuobiAccountInfo>("get_account_info");
        }

        public override HuobiOrderResult OnBuy(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            return Send<HuobiOrderResult>("buy", "coin_type", (int)coinType, "price", price, "amount", amountBtc);
        }

        public override HuobiOrderResult OnSell(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            return Send<HuobiOrderResult>("sell", "coin_type", (int)coinType, "price", price, "amount", amountBtc);
        }

        public override HuobiSimpleResult CancelOrder(HuobiMarket coinType, uint uid)
        {
            return Send<HuobiSimpleResult>("cancel_order", "coin_type", (int)coinType, "id", uid);
        }

        public override DateTime GetHuobiTime()
        {
            DateTime time;
            try
            {
                var error = Send<HuobiError>("order_info", "id", 0);
                time = UnixTime.ConvertToDateTime(error.time);
            }
            catch (HuobiException e)
            {
                time = UnixTime.ConvertToDateTime(e.m_error.time);
            }
            return time;
        }

        public override HuobiMarketSummary GetMarketSummary(HuobiMarket market)
        {
            var request =
                new SynchronousJsonWebRequest<HuobiMarketSummary>(
                    "http://market.huobi.com/staticmarket/detail_" + market + "_json.js", null, Huobi.kGet, "", 10,
                    Huobi.kRetryCount);
            return request.Send();
        }

        public override BcwTicker GetTicker(BcwMarket market)
        {
            var request =
                new SynchronousJsonWebRequest<Dictionary<BcwMarket, BcwTicker>>("https://s2.bitcoinwisdom.com/ticker",
                    null, Huobi.kGet, "", 10, Huobi.kRetryCount);
            return request.Send()[market];
        }

        public override List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid = -1)
        {
            string query;
            if (sinceTid == -1)
            {
                query = RestHelpers.BuildGetArgs("symbol", market.ToString());
            }
            else
            {
                query = RestHelpers.BuildGetArgs("symbol", market.ToString(), "since", sinceTid);
            }
            var request = new SynchronousJsonWebRequest<List<BcwTrade>>("https://s2.bitcoinwisdom.com/trades", null,
                Huobi.kGet, query, 10, Huobi.kRetryCount);

            return request.Send();
        }
    }
}