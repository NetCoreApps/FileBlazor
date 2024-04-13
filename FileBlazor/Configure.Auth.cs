using ServiceStack.Auth;
using FileBlazor.Data;
using FileBlazor.ServiceModel;

[assembly: HostingStartup(typeof(FileBlazor.ConfigureAuth))]

namespace FileBlazor;

public class ConfigureAuth : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
        {
            services.AddPlugin(new AuthFeature(IdentityAuth.For<AppUser, int>(options =>
            {
                options.SessionFactory = () => new CustomUserSession();
                options.CredentialsAuth();
            })));
        });
}
