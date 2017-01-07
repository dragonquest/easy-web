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
		HttpListenerRequest request_;

        public Uri Url { get; set; }

		public Request(HttpListenerRequest request)
		{
			request_ = request;
            Url = request.Url;
		}

        public HttpListenerRequest GetRawRequest()
        {
            return request_;
        }
	}
}
