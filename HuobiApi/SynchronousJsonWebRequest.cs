using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

using Newtonsoft.Json;

namespace HuobiApi
{
	public class RetryCountExceededException : Exception { }

	public class SynchronousJsonWebRequest<T>
	{
		public const string kGet = "GET";
		public const string kPost = "POST";

		string m_method;
		Uri m_endpoint;
		protected string m_params;
		protected JsonConverter m_converter;
		protected HttpWebRequest m_request;
		string m_receivedData;
		readonly int m_maxRetries;
		readonly int m_sleepBeforeRetry;
		int m_retries;
		int m_timeout;
		string m_retryContentType;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="converter"></param>
		/// <param name="method"></param>
		/// <param name="stringToSend"></param>
		public SynchronousJsonWebRequest(string endpoint, JsonConverter converter = null, string method = kGet, string stringToSend = "", double timeOutSeconds=10, int retries=5, double sleepBeforeRetry=0.1)
		{
			m_converter = converter;
			m_method = method;

			if (method == kGet)
			{
				endpoint += stringToSend;
			}

			m_endpoint = new Uri(endpoint);
			m_params = stringToSend;
			m_maxRetries = retries;
			m_retries = 0;
			m_timeout = (int)(timeOutSeconds * 1000);
			m_sleepBeforeRetry = (int)(sleepBeforeRetry * 1000);

			CreateRequest();
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void CreateRequest(string contentType=null)
		{
			m_request = (HttpWebRequest)WebRequest.Create(m_endpoint);
			m_request.Method = m_method;
			m_request.Timeout = m_timeout;
			m_request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
			m_request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			if (contentType != null)
			{
				m_request.ContentType = contentType;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public WebHeaderCollection Headers
		{
			get { return m_request.Headers; }
			set { m_request.Headers = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string ContentType
		{
			set { m_request.ContentType = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Accept
		{
			set { m_request.Accept = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public T Send()
		{
			string data = null;

			while (data == null && m_retries < m_maxRetries)
			{
				try
				{
					if (m_method == kPost)
					{
						Stream reqStream = m_request.GetRequestStream();

						byte[] message = Encoding.UTF8.GetBytes(m_params);

						reqStream.Write(message, 0, message.Length);
						reqStream.Close();
					}

					HttpWebResponse response = (HttpWebResponse)m_request.GetResponse();
					StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

					// deserialise the server object
					data = sr.ReadToEnd();

					sr.Close();
					response.Close();

					break;
				}
				catch (TimeoutException)
				{
					Console.WriteLine("Timeout, retrying...");
				}
				catch (WebException)
				{
					Console.WriteLine("Web exception, retrying...");
				}
				catch (IOException)
				{
					Console.WriteLine("IO Error retrying...");
				}

				m_retries++;

				if (m_sleepBeforeRetry > 0)
				{
					Thread.Sleep(m_sleepBeforeRetry);

					m_retryContentType = m_request.ContentType;
					CreateRequest(m_retryContentType);
				}
			}

			if (data == null && m_retries == m_maxRetries)
			{
				throw new RetryCountExceededException();
			}

			T serverObj = DeserialiseData(data);
						
			return serverObj;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		protected virtual T DeserialiseData(string data)
		{
			m_receivedData = data;

			if (m_converter == null)
			{
				return JsonConvert.DeserializeObject<T>(data);
			}
			else
			{
				return JsonConvert.DeserializeObject<T>(data, m_converter);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string m_ReceivedData
		{
			get { return m_receivedData; }
		}
	}	
}
