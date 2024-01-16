using ServiceStack.Auth;
using FileBlazor.Data;
using FileBlazor.ServiceModel;

[assembly: HostingStartup(typeof(FileBlazor.ConfigureAuth))]

namespace FileBlazor;

public class ConfigureAuth : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureAppHost(appHost => {
            appHost.Plugins.Add(new AuthFeature(IdentityAuth.For<AppUser,int>(options => {
                options.EnableCredentialsAuth = true;
                options.SessionFactory = () => new CustomUserSession();
            })));
        });
}
