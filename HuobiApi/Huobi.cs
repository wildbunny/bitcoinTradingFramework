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
    }
}