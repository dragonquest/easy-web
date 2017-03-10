using System;
using System.Net;

using System.IO; // FileServer
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using EasyWeb.Net.Http;
using EasyWeb.Net.Http.Handler;

using System.Threading;

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

        var tmpl = new EasyWeb.View.TemplateEngine.Razor();
        tmpl.LoadFromPath(config.HtmlTemplateBaseDir);

        var memeStorage = new Storage.MemeFileStorage(config.MemeStorageBaseDir);

        var memeTemplateStorage = new Storage.MemeTemplate();
        memeTemplateStorage.Load(config.MemeTemplateBaseDir);

        var httpServer = new Server(new EasyWeb.Log.Console());

        var memeCtrl = new Controller.MemeGen(tmpl, memeTemplateStorage, memeStorage, config.MemeWebPath);
        var assetsServer = new Controller.AssetsServer(config.AssetsBaseDir);

        httpServer.Handle("/assets/(?<file>.*)$", new StripPrefix("/assets/", assetsServer));
        httpServer.Handle("/", new HandlerFunc(memeCtrl.IndexPage));
        httpServer.Handle("/select-template", new HandlerFunc(memeCtrl.SelectTemplatePage));
        httpServer.Handle("/create-meme", new HandlerFunc(memeCtrl.CreateMemePage));
        httpServer.Handle("/save-meme", new HandlerFunc(memeCtrl.SaveMemePage));
        httpServer.NotFound(new Controller.NotFound());

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
    [DataMember(Name="html_template_base_dir")] public string HtmlTemplateBaseDir { get; set; }
    [DataMember(Name="assets_base_dir")] public string AssetsBaseDir { get; set; }
    [DataMember(Name="bind_address")] public string BindAddress { get; set; }
    [DataMember(Name="meme_template_base_dir")] public string MemeTemplateBaseDir { get; set; }
    [DataMember(Name="meme_storage_base_dir")] public string MemeStorageBaseDir { get; set; }
    [DataMember(Name="meme_web_path")] public string MemeWebPath { get; set; }
}
