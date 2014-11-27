using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BtcE;
using HuobiApi;

using Newtonsoft;

using bitcoinTradingFramework.Algorithms;

namespace bitcoinTradingFramework
{
	class Program
	{
		static void Main(string[] args)
		{
			// attach graph renderer
			Rendering renderer = new Rendering(800, 400);
            long lastTradeId = -1; 
            IMarket huobi = new Huobi("3e6fb6f8-8e459ed7-55aece07-2617b", "6594f43e-336b96f9-bb7eeedf-547a2");
            //huobi = new BTCeMarket("RUMT84PO-7SL1YXH9-IKPT5J4I-L5B8H14U-JD7TC3Y8", "acf9fec8210c75d19cbd9817b934689e4598cdb3b4fae7de2f963b4517ec4e16");
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
