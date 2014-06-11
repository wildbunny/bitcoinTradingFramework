using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace HuobiApi
{
	/// <summary>
	/// 
	/// </summary>
	public class HuobiException : Exception
	{
		public HuobiError m_error;

		public HuobiException(HuobiError error): base()
		{
			m_error = error;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class HuobiSyncWebRequest<T> : SynchronousJsonWebRequest<T>
	{
		public HuobiSyncWebRequest(	string endpoint, JsonConverter converter = null, string method = kGet, string stringToSend = "", double timeOutSeconds=30, int retrycount=5) :
			base(endpoint, converter, method, stringToSend, timeOutSeconds, retrycount)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		protected override T DeserialiseData(string data)
		{
			try
			{
				// is this an error response?
				HuobiError errorMsg = JsonConvert.DeserializeObject<HuobiError>(data);

				// call the error handler
				if (errorMsg == null || errorMsg.code == 0)
				{
					throw new JsonSerializationException();
				}

				throw new HuobiException(errorMsg);
			}
			catch (JsonSerializationException)
			{
				// couldn't deserialise the error, probably ok to proceed!
				return base.DeserialiseData(data);
			}
		}
	}
}
