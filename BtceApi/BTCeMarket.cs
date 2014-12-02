using System;
using System.Collections.Generic;
using System.Linq;
using HuobiApi;

namespace BtcE
{
    public class BTCeMarket : MarketBase
    {
        readonly BtceApi _btceApi;

        public BTCeMarket(string accessKey, string secretKey, double sleepAfterCallSeconds = 1.5)
            : base(accessKey, secretKey, sleepAfterCallSeconds)
        {
            _btceApi = new BtceApi(accessKey, secretKey);
        }

        public override HuobiOrderResult OnSell(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            TradeAnswer tradeAnswer = _btceApi.Trade(BtcePair.btc_usd, TradeType.Sell, price, amountBtc);

            return new HuobiOrderResult { id = (uint)tradeAnswer.OrderId, result = tradeAnswer.Received.ToString() };
        }

        public override HuobiSimpleResult CancelOrder(HuobiMarket coinType, uint uid)
        {
            CancelOrderAnswer cancelAnswer = _btceApi.CancelOrder((int)uid);
            return new HuobiSimpleResult {result = cancelAnswer.OrderId.ToString()};
        }

        public override HuobiAccountInfo GetAccountInfo()
        {
            UserInfo info = _btceApi.GetInfo();

            return new HuobiAccountInfo
            {
                available_btc_display = info.Funds.Btc,
            };
        }

        public override List<HuobiOrder> GetOpenOrders(HuobiMarket coinType)
        {
            OrderList orderList = _btceApi.GetOrderList();
            if (orderList.List != null)
            {
                return orderList.List.Select(e =>
                    new HuobiOrder
                    {
                        order_amount = e.Value.Amount,
                        order_price = e.Value.Rate,
                        order_time = e.Value.TimestampCreated,
                        type = e.Value.Type == TradeType.Buy ? HuobiOrderType.buy : HuobiOrderType.sell,
                    }).ToList();
            }
            return new List<HuobiOrder>();
        }

        public override HuobiOrderResult OnBuy(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            TradeAnswer tradeAnswer = _btceApi.Trade(BtcePair.btc_usd, TradeType.Buy, price, amountBtc);

            return new HuobiOrderResult {id = (uint) tradeAnswer.OrderId, result = tradeAnswer.Received.ToString()};
        }

        public override List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid = -1)
        {
            List<TradeInfo> trades = BtceApi.GetTrades(BtcePair.btc_usd);

            return trades.Select(e => new BcwTrade
            {
                amount = e.Amount,
                date = UnixTime.GetFromDateTime(e.Date),
                price = e.Price,
                tid = e.Tid,
                trade_type = e.Type == TradeInfoType.Ask
                    ? BcwOrderType.ask
                    : BcwOrderType.bid
            }).ToList();
        }

        public override HuobiMarketSummary GetMarketSummary(HuobiMarket market)
        {
            Depth btcusdDepth = BtceApi.GetDepth(BtcePair.btc_usd);
            Ticker ticker = BtceApi.GetTicker(BtcePair.btc_usd);
            List<TradeInfo> trades = BtceApi.GetTrades(BtcePair.btc_usd);

            return new HuobiMarketSummary
            {
                buys = btcusdDepth.Asks.Select(a => new HuobiDepthItem { amount = a.Amount, price = a.Price }).ToArray(),
                sells = btcusdDepth.Bids.Select(a => new HuobiDepthItem { amount = a.Amount, price = a.Price }).ToArray(),
                trades = trades.Select(a => new HuobiTransaction
                {
                    amount = a.Amount, 
                    price = a.Price,
                    time = a.Date.ToString(),
                    type = a.Type.ToString(),
                }).ToArray(),
                
                p_low = ticker.Low,
                amount = ticker.Volume,
                p_high = ticker.High,
            };
        }

        public override DateTime GetHuobiTime()
        {
            Ticker ticker = BtceApi.GetTicker(BtcePair.btc_usd);
            return UnixTime.ConvertToDateTime(ticker.ServerTime);
        }

        public override BcwTicker GetTicker(BcwMarket market)
        {
            Ticker ticker = BtceApi.GetTicker(BtcePair.btc_usd);
            return new BcwTicker
            {
                date = ticker.ServerTime,
                last = ticker.Last,
            };
        }
    }
}