using System;
using System.Net;

namespace Cms.Net.Http
{
	public interface IHandler
	{
		void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams);
	}

	public delegate void HandlerFuncCallback(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams);
	
	public class HandlerFunc : IHandler
	{
		HandlerFuncCallback func_;
		public HandlerFunc(HandlerFuncCallback func)
		{
			func_ = func;
		}

		public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
		{
			func_(response, request, urlParams);
		}
	}
}
