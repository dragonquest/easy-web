using System;
using System.Net;

using System.IO; // FileServer

using Cms.Net.Http;

class Program
{
  public static void Main()
  {
    var tmpl = new Cms.View.Template();
    tmpl.LoadFromPath("./templates/");

    var httpServer = new Server(new Cms.Log.Console());
    
    var websiteCtrl = new AboutMe.Handler.WebsiteController(tmpl);

    httpServer.Handle("(?<name>[\\w]+).html$", new AboutMe.Handler.Website());
    httpServer.Handle("assets/(?<file>.*)", new AboutMe.Handler.AssetsHandler("./assets/"));
    httpServer.Handle("^/$", new HandlerFunc(websiteCtrl.IndexPage));
    httpServer.NotFound(new AboutMe.Handler.NotFound());
    httpServer.ListenAndServe("http://localhost:8000/");
  }
}

