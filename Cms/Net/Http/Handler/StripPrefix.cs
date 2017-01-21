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
        private string _prefix;
        private IHandler _handler;

        public StripPrefix(string prefix, IHandler handler)
        {
            _prefix = prefix;
            _handler = handler;
        }

        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            if(!request.Url.AbsolutePath.StartsWith(_prefix))
            {
                _handler.ServeHttp(response, request, urlParams);
                return;
            }

            var builder = new UriBuilder(request.Url);
            builder.Path = request.Url.AbsolutePath.Substring(_prefix.Length);
            request.Url = builder.Uri;
            _handler.ServeHttp(response, request, urlParams);
        }
    }
}
