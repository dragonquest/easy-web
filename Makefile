SRCS=main.cs Cms/Net/Http/Server.cs Cms/Log/Logger.cs

build:
	mcs -reference:System.Net.Http -reference:System.Web $(SRCS)

clean:
	rm main.exe

.PHONY: clean