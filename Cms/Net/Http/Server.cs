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
                callHandler(notFoundHandler_, new UrlParamsEmpty(), context);
                return;
            }
            
            (new Cms.Benchmark.Scope()).Bench("myName", () =>
            {
                System.Console.WriteLine("What the heck");
            });
            (new Cms.Benchmark.Scope()).Bench("nothing", () =>
            {
            });
            (new Cms.Benchmark.Scope()).Bench("myName2", () =>
            {
                for(int i = 0; i < 100; i++) 
                {
                    System.Console.WriteLine("What the heck");
                }
            });


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
				handler.ServeHttp(new ResponseWriter(response), new Request(request), urlParams);
			}
			catch(Exception e)
			{
				logger_.Error(string.Format("Exception: {0}, Source: {1}, StackTrace: {2}", e.Message, e.Source, e.StackTrace));
			}
			finally
			{
				response.Close();
			}
		}
	}
}
