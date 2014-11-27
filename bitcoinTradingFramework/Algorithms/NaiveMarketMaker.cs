using System;
using HuobiApi;

namespace bitcoinTradingFramework.Algorithms
{
    public class NaiveMarketMaker : AlgoBase
    {
        private const decimal kMinTradeThresh = Huobi.kCent;

        /// <summary>
        /// </summary>
        /// <param name="huobi"></param>
        /// <param name="market"></param>
        /// <param name="renderer"></param>
        public NaiveMarketMaker(IMarket huobi, HuobiMarket market, Rendering renderer) : base(huobi, market, renderer)
        {
        }

        /// <summary>
        /// </summary>
        public override void Update(DateTime now)
        {
            base.Update(now);

            CancelOpenOrders();

            HuobiAccountInfo info = m_huobi.GetAccountInfo();
            HuobiMarketSummary summary = m_huobi.GetMarketSummary(m_market);

            decimal midPrice = (summary.GetAskPrice(0) + summary.GetBidPrice(0))*0.5M;

            CalculateProfit(now, midPrice, info);

            decimal buyPrice = Huobi.NormalisePrice(summary.GetBidPrice(0));
            decimal sellPrice = Huobi.NormalisePrice(summary.GetAskPrice(0));

            decimal amountCanBuy = Huobi.NormaliseAmount(info.available_cny_display/buyPrice);
            decimal amountCanSell = Huobi.NormaliseAmount(info.available_btc_display);

            bool canBuy = amountCanBuy >= Huobi.kMinAmount;
            bool canSell = amountCanSell >= Huobi.kMinAmount;
            bool dontTrade = sellPrice <= buyPrice + kMinTradeThresh;


            if (!dontTrade)
            {
                if (canBuy)
                {
                    // we can action a buy!
                    HuobiOrderResult result = m_huobi.Buy(m_market, buyPrice, Huobi.kMinAmount);
                    Console.WriteLine("Buy " + Huobi.kMinAmount + "BTC at " + buyPrice);

                    m_renderer.AddMarker(true, false, buyPrice, now);
                }
                if (canSell)
                {
                    // we can action a buy!
                    HuobiOrderResult result = m_huobi.Sell(m_market, sellPrice, Huobi.kMinAmount);
                    Console.WriteLine("Sell " + Huobi.kMinAmount + "BTC at " + sellPrice);

                    m_renderer.AddMarker(false, false, sellPrice, now);
                }
            }

            m_lastOpenOrders.AddRange(m_huobi.GetOpenOrders(m_market));

            m_renderer.ReformatGraph();
        }
    }
}