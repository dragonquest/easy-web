SRCS=main.cs \
     Cms/Net/Http/Server.cs \
     Cms/Log/Logger.cs \
     Cms/View/Template.cs

build:
	mcs -r:System.Net.Http -r:System.Web -r:mustache-sharp.dll $(SRCS) 

clean:
	rm main.exe

.PHONY: clean