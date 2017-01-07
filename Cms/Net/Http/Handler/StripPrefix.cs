using System;
using System.Net;

using Cms.Net.Http;

namespace Cms.Net.Http.Handler
{
    // StripPrefix returns a IHandler that serves 
    // HTTP requests by removing a given prefix from the request URL's
    // Path and invoking the passed IHandler.
    public class StripPrefix : IHandler
    {
        private string prefix_;
        private IHandler handler_;

        public StripPrefix(string prefix, IHandler handler)
        {
            prefix_ = prefix;
            handler_ = handler;
        }

		public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
            if(!request.Url.AbsolutePath.StartsWith(prefix_))
            {
                handler_.ServeHttp(response, request, urlParams);
                return;
            }

            var builder = new UriBuilder(request.Url);
            builder.Path = request.Url.AbsolutePath.Substring(prefix_.Length);
            request.Url = builder.Uri;
			handler_.ServeHttp(response, request, urlParams);
		}
    }
}
