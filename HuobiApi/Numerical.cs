using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuobiApi
{
	public class Numerical
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="precision"></param>
		/// <returns></returns>
		static public decimal TruncateDecimal(decimal value, int precision)
		{
			decimal step = (decimal)Math.Pow(10, precision);
			value = Math.Floor(step * value);
			return value / step;
		}
	}
}
