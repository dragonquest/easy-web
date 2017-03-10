SRCS = $(shell find . -name '*.cs' -and -not -name '*test*' -and -not -path './Demo/*')
SRCS_TESTS = $(shell find . -name '*.cs')
DEPS = -r:System.Net.Http -r:System.Web -r:System.Web.Extensions -r:System.Runtime.Serialization -r:ThirdParty/mustache-sharp.dll -r:ThirdParty/RazorEngine.dll -r:ThirdParty/System.Web.Razor.dll

# Dependencies sources
# mustache: https://github.com/jehugaleahsa/mustache-sharp
# RazorEngine: https://github.com/Antaris/RazorEngine

lib:
	mcs /out:Demo/ThirdParty/EasyWeb.dll -target:library $(DEPS) $(SRCS)
	cp -rf ThirdParty/* Demo/ThirdParty/

test:
	mcs /out:tests.dll -target:library -r:nunit.framework.dll $(DEPS) $(SRCS_TESTS)
	nunit-console -nologo tests.dll
	rm TestResult.xml
	rm tests.dll

.PHONY: clean
