using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoWorks.Cryptsy.Entities;

namespace CryptoWorks.Cryptsy
{
    public class Helper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coins"></param>
        /// <param name="market"></param>
        /// <returns></returns>
        public decimal CanBuyCoinsWithN(decimal coins, Market market)
        {
            var total = 0m;
            foreach (var sellorder in market.SellOrders)
            {
                if (coins < 0)
                    break;
                sellorder.
                Total += sellorder.Quantity;
                var cost = sellorder.Price * sellorder.Quantity;
                coins -= cost;
            }

            return total;
        }
    }
}
