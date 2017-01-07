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
		HandlerFuncCallback func_;
		public HandlerFunc(HandlerFuncCallback func)
		{
			func_ = func;
		}

		public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
			func_(response, request, urlParams);
		}
	}
}
