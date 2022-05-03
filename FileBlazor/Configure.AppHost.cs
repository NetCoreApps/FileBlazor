using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Funq;
using ServiceStack;
using FileBlazor.ServiceInterface;
using FileBlazor.ServiceModel.Types;
using ServiceStack.Azure.Storage;
using ServiceStack.Configuration;
using ServiceStack.IO;

[assembly: HostingStartup(typeof(FileBlazor.AppHost))]

namespace FileBlazor;

public class AppHost : AppHostBase, IHostingStartup
{
    public AppHost() : base("FileBlazor", typeof(MyServices).Assembly) { }

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig {
        });

        Plugins.Add(new CorsFeature(allowedHeaders: "Content-Type,Authorization",
            allowOriginWhitelist: new[]{
            "http://localhost:5000",
            "https://localhost:5001",
            "https://" + Environment.GetEnvironmentVariable("DEPLOY_CDN")
        }, allowCredentials: true));
        
        var wwwrootVfs = GetVirtualFileSource<FileSystemVirtualFiles>();
        var appDataVfs = new FileSystemVirtualFiles(ContentRootDirectory.RealPath.CombineWith("App_Data").AssertDir());
        var s3Client = new AmazonS3Client(
            Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
            Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), 
            RegionEndpoint.USEast1);
        var s3DataVfs = new S3VirtualFiles(s3Client, "file-blazor-demo");
        var azureBlobVfs = new AzureBlobVirtualFiles(
            Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING"),
            "file-blazor-demo");
        Plugins.Add(new FilesUploadFeature(
            new UploadLocation("azure", azureBlobVfs,
                readAccessRole: RoleNames.AllowAnon,
                resolvePath: ctx =>
                {
                    if (ctx.Dto is IAppFile { FileAccessType: { } } createFile)
                    {
                        return $"/azure/{createFile.FileAccessType}/{ctx.FileName}";
                    }
                    else
                        throw HttpError.BadRequest("Invalid file creation request.");
                    
                }),
            new UploadLocation("s3", s3DataVfs,
                readAccessRole: RoleNames.AllowAnon,
                resolvePath: ctx =>
                {
                    if (ctx.Dto is IAppFile { FileAccessType: { } } createFile)
                    {
                        return $"/aws/{createFile.FileAccessType}/{ctx.FileName}";
                    }
                    else
                        throw HttpError.BadRequest("Invalid file creation request.");
                    
                }),
            new UploadLocation("fs", appDataVfs,
                readAccessRole: RoleNames.AllowAnon,
                resolvePath: ctx =>
                {
                    if (ctx.Dto is IAppFile { FileAccessType: { } } createFile)
                    {
                        return $"/local/{createFile.FileAccessType}/{ctx.FileName}";
                    }
                    else
                        throw HttpError.BadRequest("Invalid file creation request.");
                    
                })
            
        ));
    }

    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) => 
            services.ConfigureNonBreakingSameSiteCookies(context.HostingEnvironment));
}
