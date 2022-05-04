using System.Diagnostics;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Funq;
using ServiceStack;
using FileBlazor.ServiceInterface;
using FileBlazor.ServiceModel.Types;
using ServiceStack.Auth;
using ServiceStack.Azure.Storage;
using ServiceStack.Configuration;
using ServiceStack.IO;
using ServiceStack.Web;

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
            Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? Environment.GetEnvironmentVariable("LOCAL_AWS_ACCESS_KEY_ID"),
            Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? Environment.GetEnvironmentVariable("LOCAL_AWS_SECRET_ACCESS_KEY"), 
            RegionEndpoint.USEast1);
        var s3DataVfs = new S3VirtualFiles(s3Client, "file-blazor-demo");
        var azureBlobVfs = new AzureBlobVirtualFiles(
            Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING") ?? Environment.GetEnvironmentVariable("LOCAL_AZURE_BLOB_CONNECTION_STRING"),
            "file-blazor-demo");
        Plugins.Add(new FilesUploadFeature(
            new UploadLocation("azure", azureBlobVfs,
                readAccessRole: RoleNames.AllowAnon,
                resolvePath: ResolveUploadPath,
                validateUpload: ValidateUpload,
                validateDownload: ValidateDownload),
            new UploadLocation("s3", s3DataVfs,
                readAccessRole: RoleNames.AllowAnon,
                resolvePath: ResolveUploadPath, 
                validateUpload:ValidateUpload,
                validateDownload: ValidateDownload),
            new UploadLocation("fs", appDataVfs,
                readAccessRole: RoleNames.AllowAnon,
                resolvePath: ResolveUploadPath, 
                validateUpload:ValidateUpload,
                validateDownload: ValidateDownload)
            
        ));
    }

    private static string ResolveUploadPath(FilesUploadContext ctx)
    {
        if (ctx.Dto is IAppFile { FileAccessType: { } } createFile)
        {
            return createFile.FileAccessType != FileAccessType.Private
                ? ctx.GetLocationPath($"/{createFile.FileAccessType}/{ctx.FileName}")
                : ctx.GetLocationPath($"/{createFile.FileAccessType}/{ctx.UserAuthId}/{ctx.FileName}");
        }
        else
            throw HttpError.BadRequest("Invalid file creation request.");
    }

    private static void ValidateUpload(IRequest request, IHttpFile file)
    {
        if (request.Dto is IAppFile createFile)
        {
            var accessType = createFile.FileAccessType;
            var ext = file.FileName.LastRightPart('.');
            if (accessType == FileAccessType.Gallery && ext != null && FileExt.Images.Contains(ext) == false)
            {
                throw new ArgumentException("Supported file extensions: {0}".LocalizeFmt(request, string.Join(", ",
                    FileExt.Images.Select(x => '.' + x).ToList().OrderBy(x => x))), file.FileName);
            }
        }
        else
            throw new HttpError("Invalid request.");


    }

    private static void ValidateDownload(IRequest request, IVirtualFile file)
    {
        var session = request.GetSession();
        // Admin role has access to all files.
        if(session?.HasRole(RoleNames.Admin, request.Resolve<IAuthRepository>()) == true)
            return;
        if (!GetFileAccessAndUserIdFromPath(file.VirtualPath, out var accessUserIdTuple))
        {
            var logger = request.Resolve<ILoggerFactory>().CreateLogger<AppHost>();
            logger.LogWarning($"Invalid file download path request: {request?.PathInfo}");
            throw HttpError.NotFound("File not found.");
        }

        var accessType = accessUserIdTuple!.Item2;
        var userAuthId = accessUserIdTuple.Item1;
        
        switch (accessType)
        {
            case FileAccessType.Private:
                if (userAuthId != null && session?.UserAuthId == userAuthId)
                {
                    return;
                }
                throw HttpError.NotFound("File not found.");
            case FileAccessType.Gallery:
                if (session?.IsAuthenticated == true)
                {
                    return;
                }
                throw HttpError.NotFound("File not found.");
            case FileAccessType.Public:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static bool GetFileAccessAndUserIdFromPath(string virtualFilePath, out Tuple<string?, FileAccessType>? tuple)
    {
        var segments = virtualFilePath.Split("/");
        tuple = null;
        if (segments.Length < 3)
            return false;
        var access = segments[1];
        var firstSeg = segments[2];
        var accessType = access.ToEnum<FileAccessType>();
        tuple = new Tuple<string?, FileAccessType>(accessType == FileAccessType.Private ? firstSeg : null, accessType);
        return true;
    }

    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) => 
            services.ConfigureNonBreakingSameSiteCookies(context.HostingEnvironment));
}
