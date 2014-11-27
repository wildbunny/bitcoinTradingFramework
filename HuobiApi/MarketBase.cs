using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace HuobiApi
{
    public class MarketBase : IMarket
    {
        protected readonly string m_accessKey;
        protected readonly string m_secretKey;

        protected readonly Dictionary<string, DateTime> m_lastCalled;

        protected readonly double m_sleepAfterCallSeconds;
        protected TimeSpan m_timeOffset;

        /// <summary>
        /// </summary>
        /// <param name="huobiTime"></param>
        public MarketBase(string accessKey, string secretKey, double sleepAfterCallSeconds = 1.5)
        {
            m_accessKey = accessKey;
            m_secretKey = secretKey;

            m_timeOffset = GetHuobiTime() - DateTime.UtcNow;
            m_sleepAfterCallSeconds = sleepAfterCallSeconds;

            m_lastCalled = new Dictionary<string, DateTime>();
        }

        /// <summary>
        /// </summary>
        /// <param name="market"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public BcwMarketDepth GetDepth(BcwMarket market)
        {
            var request = new SynchronousJsonWebRequest<BcwMarketDepthResult>("https://s2.bitcoinwisdom.com/depth", null,
                Huobi.kGet, RestHelpers.BuildGetArgs("symbol", market.ToString()), 10, Huobi.kRetryCount);
            return request.Send().@return;
        }

        /// <summary>
        /// </summary>
        /// <param name="market"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid = -1)
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

        /// <summary>
        /// </summary>
        /// <param name="market"></param>
        /// <returns></returns>
        public HuobiMarketSummary GetMarketSummary(HuobiMarket market)
        {
            var request =
                new SynchronousJsonWebRequest<HuobiMarketSummary>(
                    "http://market.huobi.com/staticmarket/detail_" + market + "_json.js", null, Huobi.kGet, "", 10,
                    Huobi.kRetryCount);
            return request.Send();
        }

        /// <summary>
        /// </summary>
        /// <param name="market"></param>
        /// <returns></returns>
        public BcwTicker GetTicker(BcwMarket market)
        {
            var request =
                new SynchronousJsonWebRequest<Dictionary<BcwMarket, BcwTicker>>("https://s2.bitcoinwisdom.com/ticker",
                    null, Huobi.kGet, "", 10, Huobi.kRetryCount);
            return request.Send()[market];
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public DateTime GetHuobiTime()
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

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public HuobiAccountInfo GetAccountInfo()
        {
            return Send<HuobiAccountInfo>("get_account_info");
        }

        /// <summary>
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        public List<HuobiOrder> GetOpenOrders(HuobiMarket coinType)
        {
            return Send<List<HuobiOrder>>("get_orders", "coin_type", (int) coinType);
        }

        /// <summary>
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="price"></param>
        /// <param name="amountBtc"></param>
        /// <returns></returns>
        public HuobiOrderResult Buy(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            ValidatePrice(price);
            ValidateAmount(amountBtc);
            return Send<HuobiOrderResult>("buy", "coin_type", (int) coinType, "price", price, "amount", amountBtc);
        }

        /// <summary>
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="price"></param>
        /// <param name="amountBtc"></param>
        /// <returns></returns>
        public HuobiOrderResult Sell(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            ValidatePrice(price);
            ValidateAmount(amountBtc);
            return Send<HuobiOrderResult>("sell", "coin_type", (int) coinType, "price", price, "amount", amountBtc);
        }

        /// <summary>
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public HuobiSimpleResult CancelOrder(HuobiMarket coinType, uint uid)
        {
            return Send<HuobiSimpleResult>("cancel_order", "coin_type", (int) coinType, "id", uid);
        }

        /// <summary>
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public static decimal NormalisePrice(decimal price)
        {
            return Numerical.TruncateDecimal(price, 2);
        }

        /// <summary>
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static decimal NormaliseAmount(decimal amount)
        {
            return Numerical.TruncateDecimal(amount, 4);
        }



        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="query"></param>
        /// <param name="method"></param>
        /// <param name="timeOutSeconds"></param>
        /// <param name="retries"></param>
        /// <returns></returns>
        public virtual T2 Send<T2>(string method, params object[] args)
        {
            return default(T2);
        }

        /// <summary>
        /// </summary>
        /// <param name="price"></param>
        private void ValidatePrice(decimal price)
        {
            decimal trucated = Numerical.TruncateDecimal(price, 2);
            WildLog.Assert(trucated == price, "Price should have 2 decimal places max");
        }

        /// <summary>
        /// </summary>
        /// <param name="amount"></param>
        public virtual void ValidateAmount(decimal amount)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private string GetParamsString(object[] args)
        {
            string result = "";
            foreach (object o in args)
            {
                result += o + ",";
            }
            return result.TrimEnd(',');
        }

        /// <summary>
        /// </summary>
        /// <param name="secret_key"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected string MD5(string input)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashedBytes = System.Security.Cryptography.MD5.Create().ComputeHash(asciiBytes);
            string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedString;
        }
    }
}