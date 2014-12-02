using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CryptoWorks.Cryptsy.Entities;
using HuobiApi;

namespace CryptoWorks.Cryptsy
{
    public class CryptsyMarket : MarketBase
    {
        public CryptsyMarket(string accessKey, string secretKey, double sleepAfterCallSeconds = 1.5)
            : base(accessKey, secretKey, sleepAfterCallSeconds)
        {
            CryptsyApi.PublicKey = accessKey;
            CryptsyApi.PrivateKey = secretKey;
        }

        public override List<HuobiOrder> GetOpenOrders(HuobiMarket coinType)
        {
            List<Order> myOrders = CryptsyApi.GetAllMyOrders();
            return myOrders.Select(o => new HuobiOrder
            {
                id = uint.Parse(o.Orderid),
                order_amount = o.Quantity,
                order_price = o.Price,
                order_time = UnixTime.GetFromDateTime(o.Created),
                type =
                    o.Ordertype == Enum.GetName(typeof (OrderType), OrderType.Buy)
                        ? HuobiOrderType.buy
                        : HuobiOrderType.sell,
                processed_amount = o.Total,
            }).ToList();
        }

        public override HuobiOrderResult OnBuy(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            OrderResponse res = CryptsyApi.CreateOrder("2", OrderType.Buy, amountBtc, price);
            return new HuobiOrderResult
            {
                id = uint.Parse(res.Orderid),
                result = res.Moreinfo,
            };
        }

        public override HuobiOrderResult OnSell(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            OrderResponse res = CryptsyApi.CreateOrder("2", OrderType.Sell, amountBtc, price);
            return new HuobiOrderResult
            {
                id = uint.Parse(res.Orderid),
                result = res.Moreinfo,
            };
        }

        public override void CancelAllOrders()
        {
            CryptsyApi.CancellAllOrders();
        }

        public override HuobiSimpleResult CancelOrder(HuobiMarket coinType, uint uid)
        {
            List<string> res = CryptsyApi.CancellAllOrders();
            return new HuobiSimpleResult
            {
                result = string.Join(",", res),
            };
        }

        public override List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid = -1)
        {
            List<MarketTrade> res = CryptsyApi.GetMarketTrades("2");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            return res.Select(t =>
            {
                var dt = DateTime.Parse(t.Datetime);
                var ud = UnixTime.GetFromDateTime(dt);

                var tr = new BcwTrade
                {
                    price = decimal.Parse(t.Tradeprice),
                    tid = long.Parse(t.Tradeid),
                    amount = decimal.Parse(t.Quantity),
                    trade_type = t.InitiateOrdertype == "buy" ? BcwOrderType.ask : BcwOrderType.bid,
                    date = ud,
                };

                return tr;
            }).ToList();
        }

        public override BcwTicker GetTicker(BcwMarket market)
        {
            Market ticker = CryptsyApi.GetSingleMarket("2");
            return new BcwTicker
            {
                date = UnixTime.GetFromDateTime(GetHuobiTime()),
                last = ticker.HighestBuyPrice,
            };
        }

        public override DateTime GetHuobiTime()
        {
            CryptsyApi.PublicKey = m_accessKey;
            CryptsyApi.PrivateKey = m_secretKey;

            Info info = CryptsyApi.GetInfo();
            return DateTime.Parse(info.Serverdatetime);
        }

        public override HuobiAccountInfo GetAccountInfo()
        {
            Info info = CryptsyApi.GetInfo();
            return new HuobiAccountInfo
            {
                available_btc_display = info.BalancesAvailable.BTC,
                available_ltc_display = info.BalancesAvailable.LTC,
            };
        }

        public override HuobiMarketSummary GetMarketSummary(HuobiMarket market)
        {
            //BTC/USD
            Market cryptyMarket = CryptsyApi.GetMarkets("2").First();
            List<Trade> trades = CryptsyApi.GetTrades("2");

            return new HuobiMarketSummary
            {
                sells = cryptyMarket.SellOrders.Select(e => new HuobiDepthItem
                {
                    price = e.Price,
                    amount = e.Quantity,
                }).ToArray(),
                buys = cryptyMarket.BuyOrders.Select(e => new HuobiDepthItem
                {
                    price = e.Price,
                    amount = e.Quantity,
                }).ToArray(),
                trades = trades.Select(t => new HuobiTransaction
                {
                    amount = decimal.Parse(t.Quantity),
                    price = decimal.Parse(t.Tradeprice),
                    type = t.Tradetype,
                    time = t.Datetime,
                }).ToArray(),
                p_low = cryptyMarket.LowestSellPrice,
                p_high = cryptyMarket.HighestBuyPrice,
            };
        }
    }
}