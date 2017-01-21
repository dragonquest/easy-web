using NUnit.Framework;

using Cms.Net.Http;

[TestFixture]
public class RouterTest
{
    [Test]
    public void DelimiterSegmenter()
    {
        var expected = new string[]{"", "hello", "world"};
        var seg = new DelimiterSegmenter('/');
        var parts = seg.Split("/hello/world");
        Assert.That(expected, Is.EquivalentTo(parts));
    }

    [Test]
    public void StaticRoute()
    {
        var route = new StaticRoute();
        route.AddChild("hello", new StaticRoute());
        route.AddChild("world", new StaticRoute());

        Assert.True(route.HasChild("hello"));
        Assert.True(route.HasChild("world"));
        Assert.False(route.HasChild("unknown"));
    }

    [Test]
    public void RegexRoute()
    {
        var route = new RegexRoute();
        route.AddChild("([a-z]+)", new StaticRoute());
        route.AddChild("world", new StaticRoute());

        Assert.True(route.TryMatch("hello").IsMatch);
        Assert.False(route.TryMatch("1234").IsMatch);
    }

    [Test]
    public void RouterAddLookup()
    {
        var router = new Router(new RegexRoute(), new DelimiterSegmenter('/'));
        router.Add("/", new HandlerFake("Index"));
        router.Add("/name/peter", new HandlerFake("Peter"));
        router.Add("/assets/(?<file>.*)$", new HandlerFake("Assets"));

        var lookupRes = router.Lookup("/");
        var fake = lookupRes.Key as HandlerFake;
        Assert.AreEqual("Index", fake.GetName());

        lookupRes = router.Lookup("/name/peter");
        fake = lookupRes.Key as HandlerFake;
        Assert.AreEqual("Peter", fake.GetName());

        lookupRes = router.Lookup("/age/peter");
        Assert.IsNull(lookupRes.Key);

        // Testing the Stop (Parsing) symbol $.
        // Whenever a $ is encountered as the last char
        // then it will stop trying to find more children.
        // This is helpful for serving files & assets which may
        // contain further slashes (or specified delimiters)
        lookupRes = router.Lookup("/assets/css/style.css");
        fake = lookupRes.Key as HandlerFake;
        Assert.AreEqual("Assets", fake.GetName());
    }

}
