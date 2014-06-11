using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Threading;

using Newtonsoft.Json;

namespace HuobiApi
{
	

	public class Huobi
	{
		public const decimal kCent = 0.01M;
		public const decimal kMinAmount = 0.001M;
		const string kApiRoot = "https://api.huobi.com/apiv2.php";
		public const string kGet = "GET";
		public const string kPost = "POST";
		public const int kRetryCount = 10;

		string m_accessKey;
		string m_secretKey;
		
		TimeSpan m_timeOffset;
		double m_sleepAfterCallSeconds;
		Dictionary<string, DateTime> m_lastCalled;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="huobiTime"></param>
		public Huobi(string accessKey, string secretKey, double sleepAfterCallSeconds=1.5)
		{
			m_accessKey = accessKey;
			m_secretKey = secretKey;
			
			m_timeOffset = GetHuobiTime() - DateTime.UtcNow;
			m_sleepAfterCallSeconds = sleepAfterCallSeconds;

			m_lastCalled = new Dictionary<string, DateTime>();
		}

		/// <summary>
		/// 
		/// </summary>
		public TimeSpan m_HuobiTimeOffset
		{
			get { return m_timeOffset; }
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="market"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public BcwMarketDepth GetDepth(BcwMarket market)
		{
			SynchronousJsonWebRequest<BcwMarketDepthResult> request = new SynchronousJsonWebRequest<BcwMarketDepthResult>("https://s2.bitcoinwisdom.com/depth", null, kGet, RestHelpers.BuildGetArgs("symbol", market.ToString()), 10, kRetryCount);
			return request.Send().@return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="market"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid=-1)
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
			SynchronousJsonWebRequest<List<BcwTrade>> request = new SynchronousJsonWebRequest<List<BcwTrade>>("https://s2.bitcoinwisdom.com/trades", null, kGet, query, 10, kRetryCount);

			return request.Send();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="market"></param>
		/// <returns></returns>
		public HuobiMarketSummary GetMarketSummary(HuobiMarket market)
		{
			SynchronousJsonWebRequest<HuobiMarketSummary> request = new SynchronousJsonWebRequest<HuobiMarketSummary>("http://market.huobi.com/staticmarket/detail_" + market + "_json.js", null, kGet, "", 10, kRetryCount);
			return request.Send();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="market"></param>
		/// <returns></returns>
		public BcwTicker GetTicker(BcwMarket market)
		{
			SynchronousJsonWebRequest<Dictionary<BcwMarket, BcwTicker>> request = new SynchronousJsonWebRequest<Dictionary<BcwMarket, BcwTicker>>("https://s2.bitcoinwisdom.com/ticker", null, kGet, "", 10, kRetryCount);
			return request.Send()[market];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public DateTime GetHuobiTime()
		{
			DateTime time;
			try
			{
				HuobiError error = Send<HuobiError>("order_info", "id", 0);
				time = UnixTime.ConvertToDateTime(error.time);
			}
			catch (HuobiException e)
			{
				time = UnixTime.ConvertToDateTime(e.m_error.time);
			}
			return time;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public HuobiAccountInfo GetAccountInfo()
		{
			return Send<HuobiAccountInfo>("get_account_info");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="coinType"></param>
		/// <returns></returns>
		public List<HuobiOrder> GetOpenOrders(HuobiMarket coinType)
		{
			return Send<List<HuobiOrder>>("get_orders", "coin_type", (int)coinType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="coinType"></param>
		/// <param name="price"></param>
		/// <param name="amountBtc"></param>
		/// <returns></returns>
		public HuobiOrderResult Buy(HuobiMarket coinType, decimal price, decimal amountBtc)
		{
			ValidatePrice(price);
			ValidateAmount(amountBtc);
			return Send<HuobiOrderResult>("buy", "coin_type", (int)coinType, "price", price, "amount", amountBtc);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="coinType"></param>
		/// <param name="price"></param>
		/// <param name="amountBtc"></param>
		/// <returns></returns>
		public HuobiOrderResult Sell(HuobiMarket coinType, decimal price, decimal amountBtc)
		{
			ValidatePrice(price);
			ValidateAmount(amountBtc);
			return Send<HuobiOrderResult>("sell", "coin_type", (int)coinType, "price", price, "amount", amountBtc);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="price"></param>
		void ValidatePrice(decimal price)
		{
			decimal trucated = Numerical.TruncateDecimal(price, 2);
			WildLog.Assert(trucated == price, "Price should have 2 decimal places max");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		void ValidateAmount(decimal amount)
		{
			decimal trucated = Numerical.TruncateDecimal(amount, 4);
			WildLog.Assert(trucated == amount, "Amount should have 4 decimal places max");

			WildLog.Assert(amount >= kMinAmount, "Minimum tradable amount is " + kMinAmount);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="price"></param>
		/// <returns></returns>
		static public decimal NormalisePrice(decimal price)
		{
			return Numerical.TruncateDecimal(price, 2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		static public decimal NormaliseAmount(decimal amount)
		{
			return Numerical.TruncateDecimal(amount, 4);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="coinType"></param>
		/// <param name="uid"></param>
		/// <returns></returns>
		public HuobiSimpleResult CancelOrder(HuobiMarket coinType, uint uid)
		{
			return Send<HuobiSimpleResult>("cancel_order", "coin_type", (int)coinType, "id", uid);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="endpoint"></param>
		/// <param name="query"></param>
		/// <param name="method"></param>
		/// <param name="timeOutSeconds"></param>
		/// <param name="retries"></param>
		/// <returns></returns>
		T2 Send<T2>(string method, params object[] args)
		{
			// add some more args for authentication
			uint seconds = UnixTime.GetFromDateTime(DateTime.UtcNow) + (uint)m_timeOffset.TotalSeconds;

			Dictionary<string, string> moreArgs = new Dictionary<string, string>
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

			var sortedByKey = moreArgs.OrderBy(kvp => kvp.Key).ToList();

			string hashArgs = RestHelpers.BuildPostArgs(sortedByKey);
			string paramsHash = MD5(hashArgs);

			sortedByKey.Add(new KeyValuePair<string, string>("sign", paramsHash));

			// remove secret key from query params
			sortedByKey.Remove(sortedByKey[sortedByKey.Count - 2]);

			string query = RestHelpers.BuildPostArgs(sortedByKey);

			HuobiSyncWebRequest<T2> request = new HuobiSyncWebRequest<T2>(kApiRoot, null, kPost, query, 10, kRetryCount);
			request.ContentType = "application/x-www-form-urlencoded";

			bool done=false;

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
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		string GetParamsString(object[] args)
		{
			string result = "";
			foreach (object o in args)
			{
				result += o + ",";
			}
			return result.TrimEnd(',');
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="secret_key"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		private string MD5(string input) 
		{
			byte[] asciiBytes = ASCIIEncoding.ASCII.GetBytes(input);
			byte[] hashedBytes = MD5CryptoServiceProvider.Create().ComputeHash(asciiBytes);
			string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
			return hashedString;
		}
	}
}
