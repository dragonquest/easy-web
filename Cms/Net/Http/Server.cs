using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;

using Cms.Log;

namespace Cms.Net.Http
{
	public interface IUrlParams
	{
		string Get(string key);
	}

	class UrlParamsEmpty : IUrlParams
	{
		public string Get(string key)
		{
			return "";
		}
	}

	class UrlParamsBag : IUrlParams
	{
		protected Dictionary<string, string> params_;

		public UrlParamsBag()
		{
			params_ = new Dictionary<string, string>();
		}

		public void Add(string key, string val)
		{
			params_.Add(key, val);
		}
		
		public string Get(string key)
		{
			if(!params_.ContainsKey(key))
			{
				return "";
			}
			return params_[key];
		}
	}

	public interface IHandler 
	{
		void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams);
	}

	class ResponseWriter
	{
		HttpListenerResponse response_;

		public ResponseWriter(HttpListenerResponse response)
		{
			response_ = response;
		}

		public void WriteString(string content) 
		{
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
			Write(buffer);
		}

		public void Write(byte[] content)
		{
			response_.ContentLength64 = content.Length;
			System.IO.Stream output = response_.OutputStream;
			output.Write(content, 0, content.Length);
			output.Close();
		}
	}

	class Server
	{
		protected Dictionary<string, IHandler> handlers_;
		protected IHandler notFoundHandler_;
		protected ILogger logger_;

		public Server(ILogger logger)
		{
			handlers_ = new Dictionary<string, IHandler>();
			logger_ = logger;
		}

		public void Handle(string url, IHandler handler)
		{
			handlers_.Add(url, handler);
		}

		public void NotFound(IHandler notFoundHandler)
		{
			notFoundHandler_ = notFoundHandler;
		}

		public void ListenAndServe(string address)
		{
			HttpListener listener = new HttpListener();
			listener.Prefixes.Add(address);
			listener.Start();

			for(;;) {
				HttpListenerContext context = listener.GetContext();

				// FIXME(an): A threadpool would be probably better:
				Thread handler = new Thread(() => route(context));
				handler.Start();
			}
		}

		protected void route(HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			var handlerUrlParameterPair = findHandler(System.Web.HttpUtility.UrlDecode(request.RawUrl));

			if(handlerUrlParameterPair.Key != null) {
				logger_.Info(string.Format("{0} - {1} {2} - {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
					                       request.HttpMethod, request.RawUrl, request.RemoteEndPoint.ToString()));
				handlerUrlParameterPair.Key.ServeHttp(response, request, handlerUrlParameterPair.Value);
				
				return;
			}

			notFoundHandler_.ServeHttp(response, request, new UrlParamsEmpty());
		}

		protected KeyValuePair<IHandler, IUrlParams> findHandler(string url)
		{
			// Iterating through each handler to check if a handler matches
			// is potentially slow but for this demo it is good enough.
			// If there are any performance issue we can optimize it after profiling.

			// TODO(an): Also check if accessing handlers_ is threadsafe or not.
			foreach(var handler in handlers_)
			{
			    Regex regex = new Regex(handler.Key);
			    var match = regex.Match(url);

			    if(match.Success)
			    {
			    	var parameters = new UrlParamsBag();

			    	// Accessing regex here again feels a bit hackish but 
			    	// based on https://msdn.microsoft.com/en-us/library/6h453d2h(v=vs.110).aspx
			    	// the regex should be thread-safe:
				    foreach(string key in regex.GetGroupNames())
				    {
				    	parameters.Add(key.ToString(), match.Groups[key].Value);
				    }
			    	return new KeyValuePair<IHandler,IUrlParams>(handler.Value, parameters);
			    }
			}

			return new KeyValuePair<IHandler, IUrlParams>(null, null);
		}
	}
}