using System;

namespace HuobiApi
{
    public interface IMarket
    {
        HuobiOrderResult Buy(HuobiMarket coinType, decimal price, decimal amountBtc);
        HuobiSimpleResult CancelOrder(HuobiMarket coinType, uint uid);
        HuobiAccountInfo GetAccountInfo();
        BcwMarketDepth GetDepth(BcwMarket market);
        DateTime GetHuobiTime();
        HuobiMarketSummary GetMarketSummary(HuobiMarket market);
        System.Collections.Generic.List<HuobiOrder> GetOpenOrders(HuobiMarket coinType);
        System.Collections.Generic.List<BcwTrade> GetPublicTrades(BcwMarket market, long sinceTid = -1);
        BcwTicker GetTicker(BcwMarket market);
        HuobiOrderResult Sell(HuobiMarket coinType, decimal price, decimal amountBtc);
    }
}
