using NUnit.Framework;
using ServiceStack;
using ServiceStack.Testing;
using FileBlazor.ServiceInterface;
using FileBlazor.ServiceModel;

namespace FileBlazor.Tests;

public class UnitTest
{
    private readonly ServiceStackHost appHost;

    public UnitTest()
    {
        appHost = new BasicAppHost();
        appHost.Container.AddTransient<MyServices>();
        appHost.Container.AddPlugin(new AutoQueryFeature());
        appHost.Init();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => appHost.Dispose();

    [Test]
    public void Can_call_MyServices()
    {
        var service = appHost.Container.Resolve<MyServices>();

        var response = (HelloResponse)service.Any(new Hello { Name = "World" });

        Assert.That(response.Result, Is.EqualTo("Hello, World!"));
    }
}