using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HuobiApi;

using Newtonsoft;

using bitcoinTradingFramework.Algorithms;
using Newtonsoft.Json;

namespace bitcoinTradingFramework
{
    class Program
	{
		static void Main()
		{
			// attach graph renderer
			Rendering renderer = new Rendering(800, 400);
            long lastTradeId = -1;

		    var marketAccesses =
                JsonConvert.DeserializeObject<List<MarketAccess>>(File.ReadAllText("accessKeys.txt"));

		    var huobiAccess = marketAccesses.First(e => e.Name == "Huobi");
            var btceAccess = marketAccesses.First(e => e.Name == "BTCE");
            var cryptsyAccess = marketAccesses.First(e => e.Name == "Cryptsy");

            IMarket huobi = new Huobi(huobiAccess.AccessKey, huobiAccess.SecretKey);
            //huobi = new BTCeMarket(btceAccess.AccessKey, btceAccess.SecretKey);
            //huobi = new CryptsyMarket(cryptsyAccess.AccessKey, cryptsyAccess.SecretKey);
       
			AlgoBase alogo = new NaiveMarketMaker(huobi, HuobiMarket.btc, renderer);
			BcwTrade lastTrade = null;
			TimeSpan timeOffset = new TimeSpan();
			DateTime lastTime = new DateTime();

			while (true)
			{
				try
				{
					List<BcwTrade> newTrades = huobi.GetPublicTrades(BcwMarket.huobibtccny, lastTradeId);
					newTrades.Reverse();

					HuobiMarketSummary depth = huobi.GetMarketSummary(HuobiMarket.btc);
					BcwTicker ticker = huobi.GetTicker(BcwMarket.huobibtccny);
					DateTime now = UnixTime.ConvertToDateTime(ticker.date) + timeOffset;

					if (newTrades.Count > 0)
					{
						if (timeOffset.TotalSeconds == 0)
						{
							DateTime firstTradeDate = UnixTime.ConvertToDateTime(newTrades[0].date);
							if (firstTradeDate < lastTime)
							{
								timeOffset = firstTradeDate - lastTime;
							}
						}

						foreach (BcwTrade t in newTrades)
						{
							if (t.trade_type == BcwOrderType.ask)
							{
								// this condition means that a BUY ORDER was filled
							}
							else
							{
								// this condition means that a SELL ORDER was filled
							}

							renderer.AddDataPoint(depth.GetBidPrice(0), depth.GetAskPrice(0), t.price, UnixTime.ConvertToDateTime(t.date));
						}

						lastTrade = newTrades.Last();
						lastTradeId = newTrades.Last().tid;
						now = UnixTime.ConvertToDateTime(lastTrade.date);
					}
					else
					{
						renderer.AddDataPoint(depth.GetBidPrice(0), depth.GetAskPrice(0), lastTrade.price, now);
					}
				

					//
					// update the algorithm
					//

					alogo.Update(now);
				}
				catch (HuobiApi.RetryCountExceededException)
				{
				}
				catch (Newtonsoft.Json.JsonReaderException e)
				{
					Console.WriteLine(e.ToString());
				}

				Thread.Sleep(5000);
			}		
		}
	}
}
