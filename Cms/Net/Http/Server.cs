using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using System.Net;

using Cms.Log;

namespace Cms.Net.Http
{
	class Server
	{
		protected Router router_;
		protected IHandler notFoundHandler_;
		protected ILogger logger_;

		public Server(ILogger logger)
		{
			router_ = new Router(new RegexRoute(), new DelimiterSegmenter('/'));
            logger_ = logger;
		}

		public void Handle(string url, IHandler handler)
		{
			router_.Add(url, handler);
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
				Task.Run(() => route(context));
			}
		}

        protected void route(HttpListenerContext context)
        {
            var handlerWithParameters = router_.Lookup(System.Web.HttpUtility.UrlDecode(context.Request.Url.AbsolutePath));

            if (handlerWithParameters.Key == null)
            {
			    notFoundHandler_.ServeHttp(new ResponseWriter(context.Response), context.Request, new UrlParamsEmpty());
                return;
            }

            callHandler(handlerWithParameters.Key, handlerWithParameters.Value, context);
            return;
        }

		protected void callHandler(IHandler handler, IUrlParams urlParams, HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			logger_.Info(string.Format("{0} - {1} - {2} {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
				                       request.RemoteEndPoint.ToString(), request.HttpMethod, request.RawUrl));

			try
			{
				handler.ServeHttp(new ResponseWriter(response), request, urlParams);
			}
			catch(Exception e)
			{
				logger_.Error(string.Format("Exception: {0}", e.Message));
			}
			finally
			{
				// TODO(an): Add a configurable error handler here,
				// so we can show to the visitor that something went wrong.
				response.Close();
			}
		}

/*		protected KeyValuePair<IHandler, IUrlParams> findHandler(string url)
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
*/
	}
}
