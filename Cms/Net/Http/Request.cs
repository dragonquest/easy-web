using System;
using System.Net;

namespace Cms.Net.Http
{
    public interface IRequest
    {
        HttpListenerRequest GetRawRequest();
        Uri Url { get; set; }
    }

    class Request : IRequest
    {
        HttpListenerRequest _request;

        public Uri Url { get; set; }

        public Request(HttpListenerRequest request)
        {
            _request = request;
            Url = request.Url;
        }

        public HttpListenerRequest GetRawRequest()
        {
            return _request;
        }
    }
}
