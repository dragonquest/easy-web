SRCS = $(shell find . -name '*.cs' -and -not -name '*test*')
SRCS_TESTS = $(shell find . -name '*.cs')
DEPS = -r:System.Net.Http -r:System.Web -r:System.Web.Extensions -r:System.Runtime.Serialization -r:ThirdParty/mustache-sharp.dll -r:ThirdParty/RazorEngine.dll -r:ThirdParty/System.Web.Razor.dll -r:ThirdParty/EasyWeb.dll

# Dependencies sources
# mustache: https://github.com/jehugaleahsa/mustache-sharp
# RazorEngine: https://github.com/Antaris/RazorEngine

build:
	mcs /out:main.exe -o- -debug $(DEPS) $(SRCS)

prod:
	mcs /out:main.exe -o+ $(DEPS) $(SRCS)

clean:
	rm main.exe.mdb
	rm main.exe

run: build
	mono --debug ./main.exe ./appconfig.json

.PHONY: clean
