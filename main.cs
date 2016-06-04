using System;
using System.Net;

using Cms.Net.Http;

class Program
{
  public static void Main()
  {
    var server = new Server();
    server.Handle("(?<name>[\\w]+).html$", new Website());
    server.NotFound(new NotFound());
    server.ListenAndServe("http://localhost:8000/");
  }
}

public class Website : IHandler
{
  public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
  {
    Console.WriteLine("{0} {1} {2}", request.HttpMethod, request.RawUrl, request.RemoteEndPoint.ToString());

    var resp = new ResponseWriter(response);
    resp.WriteString("<html><head><meta charset=\"UTF-8\"></head><body><h1>Hello "+urlParams.Get("name")+"</h1></body></html>");
  }
}

public class NotFound : IHandler
{
  public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
  {
    var resp = new ResponseWriter(response);
    resp.WriteString("<html><body><h1>Website not found</h1></body></html>");
  }
}