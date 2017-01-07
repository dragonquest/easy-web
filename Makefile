SRCS=main.cs \
     Cms/Net/Http/Server.cs \
     Cms/Net/Http/Router.cs \
     Cms/Net/Http/UrlParams.cs \
     Cms/Net/Http/ResponseWriter.cs \
     Cms/Net/Http/Handler.cs \
     Cms/Log/Logger.cs \
     Cms/View/Template.cs \
     AboutMe/Handler.cs

build:
	mcs -o- -r:System.Net.Http -r:System.Web -r:System.Runtime.Serialization -r:mustache-sharp.dll $(SRCS) 

prod:
	mcs -o+ -r:System.Net.Http -r:System.Web -r:System.Runtime.Serialization -r:mustache-sharp.dll $(SRCS) 

test:
	mcs -target:library -r:nunit.framework.dll testmain.cs

runtest:
	mono /Users/andi/Desktop/Backup/coding/csharp/days/03/bin/nunit3-console.exe testmain.dll --noresult --verbose --full

clean:
	rm main.exe
	rm testmain.dll

.PHONY: clean
