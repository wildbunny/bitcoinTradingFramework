using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HuobiApi
{
	class WildLog
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message"></param>
		public static void Assert(bool condition, string message="")
		{
			if (!condition)
			{
				throw new Exception(message);
			}
		}
	}
}
