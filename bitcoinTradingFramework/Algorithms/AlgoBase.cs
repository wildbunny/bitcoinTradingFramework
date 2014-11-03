using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HuobiApi;

namespace bitcoinTradingFramework.Algorithms
{
	public class AlgoBase
	{
		protected List<HuobiOrder> m_lastOpenOrders;
		protected Huobi m_huobi;
		protected HuobiMarket m_market;
		protected Rendering m_renderer;
		protected HuobiAccountInfo m_startInfo;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="huobi"></param>
		/// <param name="market"></param>
		/// <param name="renderer"></param>
		public AlgoBase(Huobi huobi, HuobiMarket market, Rendering renderer)
		{
			m_huobi = huobi;
			m_market = market;
			m_lastOpenOrders = m_huobi.GetOpenOrders(m_market);
			m_startInfo = m_huobi.GetAccountInfo();
			m_renderer = renderer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="now"></param>
		/// <param name="infoNow"></param>
		protected void CalculateProfit(DateTime now, decimal midPrice, HuobiAccountInfo infoNow)
		{
			//
			// total ROI
			//

			decimal totalBtcValueStart = m_startInfo.m_TotalCny / midPrice + m_startInfo.m_TotalBtc;
			decimal totalBtcValueNow = infoNow.m_TotalCny / midPrice + infoNow.m_TotalBtc;

		    decimal profitPercent = totalBtcValueStart > 0
		        ? 100*(totalBtcValueNow - totalBtcValueStart)/totalBtcValueStart
		        : 0;

			Console.WriteLine("profit % = " + profitPercent);

			m_renderer.AddProfitDataPoints(profitPercent, now);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		protected bool CancelOrder(HuobiOrder o)
		{
			bool traded = false;

			try
			{
				HuobiSimpleResult cancelResult = m_huobi.CancelOrder(m_market, o.id);
			}
			catch (HuobiException e)
			{
				// ignore order which have been filled, or cancelled
				if (e.m_error.code != 41 && e.m_error.code != 42)
				{
					throw;
				}
				else
				{
					if (e.m_error.code == 41)
					{
						// not found, so filled
						m_renderer.AddMarker(o.type == HuobiOrderType.buy, true, o.order_price, UnixTime.ConvertToDateTime(o.order_time));

						traded = true;
					}

					m_lastOpenOrders.RemoveAll(loo => loo.id == o.id);
				}
			}

			return traded;
		}

		/// <summary>
		/// 
		/// </summary>
		protected void CancelOpenOrders()
		{
			foreach (HuobiOrder o in m_lastOpenOrders.ToList())
			{
				CancelOrder(o);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Update(DateTime now)
		{

		}
	}
}
