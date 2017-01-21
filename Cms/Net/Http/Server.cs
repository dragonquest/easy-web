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
        protected Router _router;
        protected IHandler _notFoundHandler;
        protected ILogger _logger;

        public Server(ILogger logger)
        {
            _router = new Router(new RegexRoute(), new DelimiterSegmenter('/'));
            _logger = logger;
        }

        public void Handle(string url, IHandler handler)
        {
            _router.Add(url, handler);
        }

        public void NotFound(IHandler notFoundHandler)
        {
            _notFoundHandler = notFoundHandler;
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
            var handlerWithParameters = _router.Lookup(System.Web.HttpUtility.UrlDecode(context.Request.Url.AbsolutePath));

            if (handlerWithParameters.Key == null)
            {
                callHandler(_notFoundHandler, new UrlParamsEmpty(), context);
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

            _logger.Info(string.Format("{0} - {1} - {2} {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        request.RemoteEndPoint.ToString(), request.HttpMethod, request.RawUrl));


            try
            {
                handler.ServeHttp(new ResponseWriter(response), new Request(request), urlParams);
            }
            catch(Exception e)
            {
                _logger.Error(string.Format("Exception: {0}, Source: {1}, StackTrace: {2}", e.Message, e.Source, e.StackTrace));
            }
            finally
            {
                response.Close();
            }
        }
    }
}
