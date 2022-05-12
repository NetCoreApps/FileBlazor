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
                if(db.CreateTableIfNotExists<FileSystemFile>())
                {

                }
                if(db.CreateTableIfNotExists<S3File>())
                {

                }
                if(db.CreateTableIfNotExists<AzureFileItem>())
                {

                }
                if (db.CreateTableIfNotExists<FileSystemFileItem>())
                {
                    
                }
                if (db.CreateTableIfNotExists<S3FileItem>())
                {
                    
                }
                if (db.CreateTableIfNotExists<AzureFileItem>())
                {
                    
                }
            });
    }

    public static class ConfigureDbUtils
    {

    }
}
