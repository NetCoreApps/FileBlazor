using ServiceStack.Azure.Storage;
using ServiceStack.Data;
using ServiceStack.IO;
using ServiceStack.OrmLite;

[assembly: HostingStartup(typeof(FileBlazor.ConfigureDb))]

namespace FileBlazor;

public class ConfigureDb : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) => {
            services.AddSingleton<IDbConnectionFactory>(new OrmLiteConnectionFactory(
                context.Configuration.GetConnectionString("DefaultConnection")
                ?? ":memory:",
                SqliteDialect.Provider));
        })
        .ConfigureAppHost(appHost => {
            // Enable built-in Database Admin UI at /admin-ui/database
            // Create non-existing Table and add Seed Data Example                
            appHost.ConfigurePlugin<FilesUploadFeature>(async feature =>
            {
                using var db = await appHost.Resolve<IDbConnectionFactory>().OpenAsync();
                var s3Vfs = feature.GetLocation("s3")?.VirtualFiles as S3VirtualFiles;
                var azureVfs = feature.GetLocation("azure")?.VirtualFiles as AzureBlobVirtualFiles;
                var fsVfs = feature.GetLocation("fs")?.VirtualFiles as FileSystemVirtualFiles;
                var sampleFilePath = appHost.ContentRootDirectory.RealPath.CombineWith("Migrations").CombineWith("samplefiles").AssertDir();
                await db.SeedFileBlazor(s3Vfs, azureVfs, fsVfs, sampleFilePath);
            });
        });
}
