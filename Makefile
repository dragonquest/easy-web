SRCS = $(shell find . -name '*.cs' -and -not -name '*test*')

build:
	mcs -o- -debug -r:System.Net.Http -r:System.Web -r:System.Runtime.Serialization -r:mustache-sharp.dll $(SRCS) 

lock:
	mcs /out:lock.exe -o- -debug Cms/Benchmark/Benchmark.cs Cms/Benchmark/Experiment.cs fcklock.cs

prod:
	mcs -o+ -r:System.Net.Http -r:System.Web -r:System.Runtime.Serialization -r:mustache-sharp.dll $(SRCS) 

test:
	mcs -target:library -r:nunit.framework.dll testmain.cs
	nunit-console testmain.dll

runtest:
	mono /Users/andi/Desktop/Backup/coding/csharp/days/03/bin/nunit3-console.exe testmain.dll --noresult --verbose --full

clean:
	rm main.exe
	rm testmain.dll

.PHONY: clean
