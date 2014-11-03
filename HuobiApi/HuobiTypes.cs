using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuobiApi
{
	public enum HuobiMarket
	{
		btc=1,
		ltc
	}

	public class HuobiError
	{
		public int code;
		public string msg;
		public uint time;
	}

	public interface IOrderBook2
	{
		decimal GetAskVolume(int level);
		decimal GetAskPrice(int level);
		decimal GetBidVolume(int level);
		decimal GetBidPrice(int level);
	}

	public class BcwMarketDepth : IOrderBook2
	{
		public List<decimal[]> asks;
		public List<decimal[]> bids;

		decimal GetVolume(List<decimal[]> data, int level)
		{
			return data[level][1];
		}
		decimal GetPrice(List<decimal[]> data, int level)
		{
			return data[level][0];
		}
		public decimal GetAskVolume(int level)
		{
			return GetVolume(asks, level);
		}
		public decimal GetAskPrice(int level)
		{
			return GetPrice(asks, level);
		}
		public decimal GetBidVolume(int level)
		{
			return GetVolume(bids, level);
		}
		public decimal GetBidPrice(int level)
		{
			return GetPrice(bids, level);
		}
	}

	public class BcwMarketDepthResult
	{
		public string result;
		public BcwMarketDepth @return;
	}

	public enum BcwOrderType
	{
		bid,
		ask
	}

	public enum BcwMarket
	{
		huobibtccny,
		huobiltccny,
		bitstampbtcusd,
		btcebtcusd,
		btceltcbtc,
		btceltcusd,
        bitfinexbtcusd
	}

	public class BcwTrade
	{
		public long tid;
		public uint date;
		public decimal price;
		public decimal amount;
		public BcwOrderType trade_type;
	}

	public class BcwTicker
	{
		public decimal last;
		public uint date;
		public long tid;
	}

	public class BcwAllTickers
	{
		public BcwTicker bitstampbtcusd;
		public BcwTicker btcebtcusd;
		public BcwTicker btceltcbtc;
		public BcwTicker btceltcusd;
		public BcwTicker huobibtccny;
	}

	/// <summary>
	/// "{\"total\":\"0.00\",
	/// \"net_asset\":\"0.00\",
	/// \"available_cny_display\":\"0.00\",
	/// \"available_btc_display\":\"0.0000\",
	/// \"available_ltc_display\":\"0.0000\",
	/// \"frozen_cny_display\":\"0.00\",
	/// \"frozen_btc_display\":\"0.0000\",
	/// \"frozen_ltc_display\":\"0.0000\",
	/// \"loan_cny_display\":\"0.00\",
	/// \"loan_btc_display\":\"0.0000\",
	/// \"loan_ltc_display\":\"0.0000\"}"
	/// </summary>
	public class HuobiAccountInfo
	{
		public decimal total;//	Total assets equivalent
		public decimal net_asset;//	Net assets equivalent
		public decimal available_cny_display;//	Available yuan
		public decimal available_btc_display;//	Available bitcoin
		public decimal available_ltc_display;//	Wright credits available
		public decimal frozen_cny_display;//	Freeze yuan
		public decimal frozen_btc_display;//	Freeze bitcoin
		public decimal frozen_ltc_display;//	Wright currency freeze
		public decimal loan_cny_display;//	Number of RMB loans
		public decimal loan_btc_display;//	Borrowing number of bitcoins
		public decimal loan_ltc_display;//

		public decimal m_TradableBtc
		{
			get { return available_btc_display; }
		}
		public decimal m_OpenOrdersBtc
		{
			get { return frozen_btc_display; }
		}
		public decimal m_TotalBtc
		{
			get { return available_btc_display + frozen_btc_display; }
		}
		public decimal m_TotalCny
		{
			get { return available_cny_display + frozen_cny_display; }
		}
	}

	public enum HuobiOrderType
	{
		buy=1,
		sell
	}

	public class HuobiOrder
	{
		public uint id;
		public HuobiOrderType type;
		public decimal order_price;
		public decimal order_amount;
		public decimal processed_amount;
		public uint order_time;
	}

	public class HuobiOrderResult
	{
		public string result;
		public uint id;
	}

	public class HuobiDepthItem
	{
		public decimal price;
		public decimal amount;
		public int level;
	}

	public class HuobiDepthItemAccu : HuobiDepthItem
	{
		public decimal accu;
	}

	public class HuobiTransaction
	{
		public string time;
		public decimal price;
		public decimal amount;
		public string type;
	}

	public class HuobiSimpleResult
	{
		public string result;
	}

	public class HuobiMarketSummary : IOrderBook2
	{
		public HuobiDepthItem[] sells;
		public HuobiDepthItem[] buys;
		public HuobiTransaction[] trades;
		public decimal amount;
		public decimal level;
		public decimal p_high;
		public decimal p_last;
		public decimal p_low;
		public decimal p_open;
		public decimal p_new;
		public HuobiDepthItemAccu[] top_sell;
		public HuobiDepthItemAccu[] top_buy;

		public decimal GetAskVolume(int level)
		{
			return sells[level].amount;
		}
		public decimal GetAskPrice(int level)
		{
			return sells[level].price;
		}
		public decimal GetBidVolume(int level)
		{
			return buys[level].amount;
		}
		public decimal GetBidPrice(int level)
		{
			return buys[level].price;
		}
	}
}
