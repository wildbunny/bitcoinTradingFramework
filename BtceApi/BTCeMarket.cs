using System;
using System.Collections.Generic;
using System.Linq;
using HuobiApi;

namespace BtcE
{
    public class BTCeMarket : MarketBase
    {
        public BTCeMarket(string accessKey, string secretKey, double sleepAfterCallSeconds = 1.5)
            : base(accessKey, secretKey, sleepAfterCallSeconds)
        {
        }

        public override List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid = -1)
        {
            List<TradeInfo> trades = BtceApi.GetTrades(BtcePair.btc_usd);

            return trades.Select(e => new BcwTrade
            {
                amount = e.Amount,
                date = 0,
                price = e.Price,
                tid = e.Tid,
                trade_type = e.Type == TradeInfoType.Ask
                    ? BcwOrderType.ask
                    : BcwOrderType.bid
            }).ToList();
        }

        public override HuobiMarketSummary GetMarketSummary(HuobiMarket market)
        {
            Ticker ticker = BtceApi.GetTicker(BtcePair.btc_usd);
            return new HuobiMarketSummary
            {
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
            throw new NotImplementedException();
        }
    }
}