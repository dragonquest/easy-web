SRCS = $(shell find . -name '*.cs' -and -not -name '*test*')
SRCS_TESTS = $(shell find . -name '*.cs')
DEPS = -r:System.Net.Http -r:System.Web -r:System.Runtime.Serialization -r:mustache-sharp.dll

lib:
	mcs /out:Cms.dll -target:library $(DEPS) $(SRCS)

build:
	mcs /out:Demo/main.exe -o- -debug $(DEPS) $(SRCS) 

prod:
	mcs /out:Demo/main.exe -o+ $(DEPS) $(SRCS) 

test:
	mcs /out:tests.dll -target:library -r:nunit.framework.dll $(DEPS) $(SRCS_TESTS)
	nunit-console -nologo tests.dll
	rm TestResult.xml
	rm tests.dll

clean:
	rm Demo/main.exe

.PHONY: clean
