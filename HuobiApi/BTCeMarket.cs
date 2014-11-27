using System;
using System.Collections.Generic;

namespace HuobiApi
{
    public class BTCeMarket : IMarket
    {
        public HuobiOrderResult Buy(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            throw new NotImplementedException();
        }

        public HuobiSimpleResult CancelOrder(HuobiMarket coinType, uint uid)
        {
            throw new NotImplementedException();
        }

        public HuobiAccountInfo GetAccountInfo()
        {
            throw new NotImplementedException();
        }

        public BcwMarketDepth GetDepth(BcwMarket market)
        {
            throw new NotImplementedException();
        }

        public DateTime GetHuobiTime()
        {
            throw new NotImplementedException();
        }

        public HuobiMarketSummary GetMarketSummary(HuobiMarket market)
        {
            throw new NotImplementedException();
        }

        public List<HuobiOrder> GetOpenOrders(HuobiMarket coinType)
        {
            throw new NotImplementedException();
        }

        public List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid = -1)
        {
            throw new NotImplementedException();
        }

        public BcwTicker GetTicker(BcwMarket market)
        {
            throw new NotImplementedException();
        }

        public HuobiOrderResult Sell(HuobiMarket coinType, decimal price, decimal amountBtc)
        {
            throw new NotImplementedException();
        }
    }
}