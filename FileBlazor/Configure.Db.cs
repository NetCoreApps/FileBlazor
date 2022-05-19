using FileBlazor.ServiceModel;
using FileBlazor.ServiceModel.Types;
using ServiceStack;
using ServiceStack.Azure.Storage;
using ServiceStack.Data;
using ServiceStack.IO;
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
                appHost.ConfigurePlugin<FilesUploadFeature>(async feature =>
                {
                    using var db = appHost.Resolve<IDbConnectionFactory>().Open();
                    var s3Vfs = (S3VirtualFiles)feature.GetLocation("s3").VirtualFiles;
                    var azureVfs = (AzureBlobVirtualFiles)feature.GetLocation("azure").VirtualFiles;
                    var fsVfs = (FileSystemVirtualFiles)feature.GetLocation("fs").VirtualFiles;
                    var sampleFilePath = appHost.ContentRootDirectory.RealPath.CombineWith("samplefiles").AssertDir();
                    await db.SeedFileBlazor(s3Vfs, azureVfs, fsVfs, sampleFilePath);
                });
                
            });
    }

    public static class ConfigureDbUtils
    {

    }
}
