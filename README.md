# EasyWeb - Lightweight HTTP Framework

EasyWeb is a lightweight and simple framework for building HTTP based applications on the .NET Framework and [Mono](http://mono-project.com).

The goal was to remove all the fat the other frameworks provide by implementing highly reusable and small modular components. This allows to quickly add/provide a simple Web GUI for new or existing C# applications.

The interface and structure is inspired by Go's [net/http](https://golang.org/pkg/net/http/) package.


## Write Your Application

**Sample main.cs (see demo/main.cs):**

```csharp
class Application
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
}
```


**Sample handler (see demo/handler.cs):**

```csharp
namespace Controller 
{
	// [...]
	public void IndexPage(IResponseWriter response, IRequest request, IUrlParams urlParams)
	{
		response.WriteString(_template.Render("index.cshtml", new
        {
            PublicPath = _memeWebPath,
            Memes = Enumerable.Reverse(_meme.ListAll())
        }));
	}
	// [...]
}
```

**Sample NotFound handler (see demo/handler.cs):**

The NotFound Handler is a simple IHandler and therefore easily extendable. Example:

```csharp
namespace Controller 
{
	// [...]
	public class NotFound : IHandler
	{
		public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
			response.WriteString("<html><body><h1>Website not found</h1></body></html>");
		}
	}
	// [...]
}
```

**IHandler Interface (see EasyWeb/Net/Http/Handler.cs):**

```csharp
    public interface IHandler
    {
        void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams);
    }
```

## Meme Generator Demo Application

The repository contains a demo application for creating and storing memes incl. some templates. Please check out the directory `demo/` and run `make` to compile it and then run the demo app by `make run`.


## Motivation, Author & License
I, Andreas Näpflin, created **EasyWeb** in order to learn more about the C# programming language. Although it's simple and lightweight it has not been used in production yet so I would highly recommend to use a battle-tested framework instead. For example [Nancy](https://github.com/NancyFx/Nancy).

The project is licensed under the MIT license. Please check out the `LICENSE` file.

## TODO
* Better test coverage
* `DELETE`, `GET`, `HEAD`, `OPTIONS`, `POST`, `PUT` and `PATCH` request helpers
* Better exception handling
* Documentation
* More features in general

## Development status

I won't continue to develop this application because it was a "Learning project" only. If you find it interesting and would like to see more features or that I should continue to work on it then please create an issue (and I might consider to code for it again) :-)
