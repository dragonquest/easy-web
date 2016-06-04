using System;
using System.Net;

using System.IO; // FileServer

using Cms.Net.Http;

class Program
{
  public static void Main()
  {
    var server = new Server(new Cms.Log.Console());
    server.Handle("(?<name>[\\w]+).html$", new Website());
    server.Handle("assets/(?<file>.*)", new FileServer());
    server.NotFound(new NotFound());
    server.ListenAndServe("http://localhost:8000/");
  }
}

public class Website : IHandler
{
  public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
  {
    var resp = new ResponseWriter(response);
    resp.WriteString("<html><head><meta charset=\"UTF-8\"></head><body><h1>Hello "+urlParams.Get("name")+"</h1></body></html>");
  }
}

public class FileServer : IHandler
{
  static readonly object locker_ = new object();

  public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
  {
    lock(locker_) {
      var fileName = urlParams.Get("file");

      var resp = new ResponseWriter(response);

      if (File.Exists(fileName))
      {
        BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
        byte[] allData = reader.ReadBytes(int.MaxValue);
        reader.Close();
        resp.Write(allData);
        return;
      }

      resp.WriteString("<html><head><meta charset=\"UTF-8\"></head><body><h1>File "+urlParams.Get("file")+"</h1></body></html>");
    }
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

