using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace HuobiApi
{
	public class RestHelpers
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		static public string BuildGetArgs(params object[] args)
		{
			Dictionary<string, object> da = new Dictionary<string, object>();
			for (int i = 0; i < args.Length / 2; i++)
			{
				da[args[i * 2 + 0].ToString()] = HttpUtility.UrlEncode(args[i * 2 + 1].ToString());
			}

			// turn into a query
			string query = "?";

			foreach (KeyValuePair<string, object> kvp in da)
			{
				query += kvp.Key + "=" + kvp.Value + "&";
			}

			if (query.Length == 1)
			{
				query = "";
			}
			else
			{
				query = query.TrimEnd('&');
			}

			return query;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		static public string BuildPostArgs(params object[] args)
		{
			return BuildGetArgs(args).TrimStart('?');
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		static public string BuildPostArgs(List<KeyValuePair<string, string>> parameters)
		{
			List<string> keyValues = new List<string>();
			foreach (KeyValuePair<string,string> kvp in parameters)
			{
				keyValues.Add(kvp.Key + "=" + kvp.Value);
			}
			return String.Join("&", keyValues.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		static public string BuildPostArgs(Dictionary<string, string> parameters)
		{
			return BuildPostArgs(parameters.ToList());
		}
	}
}
