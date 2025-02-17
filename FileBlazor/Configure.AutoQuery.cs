using ServiceStack;
using ServiceStack.Data;

[assembly: HostingStartup(typeof(FileBlazor.ConfigureAutoQuery))]

namespace FileBlazor;

public class ConfigureAutoQuery : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services => {
            // Enable Audit History
            services.AddSingleton<ICrudEvents>(c =>
                new OrmLiteCrudEvents(c.GetRequiredService<IDbConnectionFactory>()));
            services.AddPlugin(new AutoQueryDataFeature());
            services.AddPlugin(new AutoQueryFeature
            {
                MaxLimit = 1000,
                //IncludeTotal = true,
            });
        })
        .ConfigureAppHost(appHost => {
            
            // For Bookings https://docs.servicestack.net/autoquery-crud-bookings
            appHost.Resolve<ICrudEvents>().InitSchema();
        });
}