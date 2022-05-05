using FileBlazor.ServiceModel;
using FileBlazor.ServiceModel.Types;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System.Data;

[assembly: HostingStartup(typeof(FileBlazor.ConfigureDb))]

namespace FileBlazor
{
    public class ConfigureDb : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder) => builder
            .ConfigureServices((context,services) => services.AddSingleton<IDbConnectionFactory>(new OrmLiteConnectionFactory(
                context.Configuration.GetConnectionString("DefaultConnection") ?? ":memory:",
                SqliteDialect.Provider)))
            .ConfigureAppHost(appHost =>
            {
                // Create non-existing Table and add Seed Data Example
                using var db = appHost.Resolve<IDbConnectionFactory>().Open();                
                if(db.CreateTableIfNotExists<FsFile>())
                {

                }
                if(db.CreateTableIfNotExists<S3File>())
                {

                }
                if(db.CreateTableIfNotExists<AzureFile>())
                {

                }
                if (db.CreateTableIfNotExists<SharedFsFile>())
                {
                    
                }
                if (db.CreateTableIfNotExists<SharedS3File>())
                {
                    
                }
                if (db.CreateTableIfNotExists<SharedAzureFile>())
                {
                    
                }
            });
    }

    public static class ConfigureDbUtils
    {

    }
}
