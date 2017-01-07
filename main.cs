using System;
using System.Net;

using System.IO; // FileServer
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using Cms.Net.Http;
using Cms.Net.Http.Handler;

class Program
{
  public static void Main(string[] args)
  {
    if(args.Length == 0)
    {
      Usage();
      return;
    }
    var config = ParseAppConfig(args[0]);

    var tmpl = new Cms.View.Template();
    tmpl.LoadFromPath(config.TemplateBaseDir);

    var httpServer = new Server(new Cms.Log.Console());

    var websiteCtrl = new AboutMe.Handler.WebsiteController(tmpl);

    httpServer.Handle("/(?<name>[\\w]+).html$", new AboutMe.Handler.Website());
    httpServer.Handle("/assets/(?<file>.*)$", new StripPrefix("/assets/", new AboutMe.Handler.AssetsHandler(config.AssetsBaseDir)));
    httpServer.Handle("/", new HandlerFunc(websiteCtrl.IndexPage));
    httpServer.NotFound(new AboutMe.Handler.NotFound());
    httpServer.ListenAndServe(config.BindAddress);
  }

  public static void Usage()
  {
    Console.Error.WriteLine("Usage: {0} <config.json>", System.AppDomain.CurrentDomain.FriendlyName);
  }

  public static AppConfig ParseAppConfig(string path)
  {
      using (Stream stream = File.OpenRead(path))
      {
          var serializer = new DataContractJsonSerializer(typeof(AppConfig));
          return (AppConfig) serializer.ReadObject(stream);
      }
  }
}

[DataContract]
class AppConfig
{
    [DataMember(Name="template_base_dir")] public string TemplateBaseDir { get; set; }
    [DataMember(Name="assets_base_dir")] public string AssetsBaseDir { get; set; }
    [DataMember(Name="bind_address")] public string BindAddress { get; set; }
}
