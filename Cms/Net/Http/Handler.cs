using System;
using System.Net;

namespace Cms.Net.Http
{
    public interface IHandler
    {
        void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams);
    }

    public delegate void HandlerFuncCallback(IResponseWriter response, IRequest request, IUrlParams urlParams);

    public class HandlerFunc : IHandler
    {
        HandlerFuncCallback _func;
        public HandlerFunc(HandlerFuncCallback func)
        {
            _func = func;
        }

        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            _func(response, request, urlParams);
        }
    }
}
