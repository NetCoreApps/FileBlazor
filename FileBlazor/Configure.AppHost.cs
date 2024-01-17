using Amazon;
using Amazon.S3;
using FileBlazor.Data;
using Funq;
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
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
        {
            // Configure ASP.NET Core IOC Dependencies
            services.AddPlugin(new CorsFeature(allowedHeaders: "Content-Type,Authorization",
                allowOriginWhitelist: new[]
                {
                    "http://localhost:5000",
                    "https://localhost:5001",
                    "https://" + Environment.GetEnvironmentVariable("DEPLOY_CDN")
                }, allowCredentials: true));

            var awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ??
                                 Environment.GetEnvironmentVariable("LOCAL_AWS_ACCESS_KEY_ID");
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ??
                                     Environment.GetEnvironmentVariable("LOCAL_AWS_SECRET_ACCESS_KEY");
            var azureBlobConnString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING") ??
                                      Environment.GetEnvironmentVariable("LOCAL_AZURE_BLOB_CONNECTION_STRING");

            var appFs = new FileSystemVirtualFiles(Instance.ContentRootDirectory.RealPath.CombineWith("App_Data").AssertDir());
            var s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.USEast1);
            var s3DataVfs = new S3VirtualFiles(s3Client, "file-blazor-demo");
            var azureBlobVfs = new AzureBlobVirtualFiles(azureBlobConnString, "fileblazordemo");

            services.AddPlugin(new FilesUploadFeature(
                new UploadLocation("azure", azureBlobVfs,
                    readAccessRole: RoleNames.AllowAnon, resolvePath: ResolveUploadPath,
                    validateUpload: ValidateUpload, validateDownload: ValidateDownload,
                    maxFileBytes: 10 * 1024 * 1024),
                new UploadLocation("s3", s3DataVfs,
                    readAccessRole: RoleNames.AllowAnon, resolvePath: ResolveUploadPath,
                    validateUpload: ValidateUpload, validateDownload: ValidateDownload,
                    maxFileBytes: 10 * 1024 * 1024),
                new UploadLocation("fs", appFs,
                    readAccessRole: RoleNames.AllowAnon, resolvePath: ResolveUploadPath,
                    validateUpload: ValidateUpload, validateDownload: ValidateDownload,
                    maxFileBytes: 10 * 1024 * 1024),
                // User profiles
                new UploadLocation("users", appFs, allowExtensions: FileExt.WebImages,
                    resolvePath: ctx => $"/profiles/users/{ctx.UserAuthId}.{ctx.FileExtension}",
                    maxFileBytes: 10 * 1024 * 1024)
            ));

        });

    public AppHost() : base("FileBlazor", typeof(MyServices).Assembly) { }

    // Configure your AppHost with the necessary configuration and dependencies your App needs
    public override void Configure()
    {
        SetConfig(new HostConfig {
            IgnorePathInfoPrefixes = { "/appsettings", "/_framework" },
        });

        ConfigurePlugin<UiFeature>(feature => feature.Info.BrandIcon = new ImageInfo {
            Svg = "/img/blazor.svg",
        });
    }
    
    private static string ResolveUploadPath(FilesUploadContext ctx)
    {
        if (ctx.Dto is IFileItemRequest { FileAccessType: { } } createFile)
        {
            return createFile.FileAccessType != FileAccessType.Private
                ? ctx.GetLocationPath($"/{createFile.FileAccessType}/{ctx.FileName}")
                : ctx.GetLocationPath($"/{createFile.FileAccessType}/{ctx.UserAuthId}/{ctx.FileName}");
        }
        throw HttpError.BadRequest("Invalid file creation request.");
    }

    private static void ValidateUpload(IRequest request, IHttpFile file)
    {
        if (request.Dto is IFileItemRequest createFile)
        {
            var accessType = createFile.FileAccessType;
            var ext = file.FileName.LastRightPart('.');
            if (accessType == FileAccessType.Team && ext != null && FileExt.WebImages.Contains(ext) == false)
                throw new ArgumentException("Supported file extensions: {0}".LocalizeFmt(request,
                    string.Join(", ", FileExt.WebImages.Map(x => '.' + x).OrderBy(x => x))), file.FileName);
        }
        else
            throw new HttpError("Invalid request.");
    }

    private static void ValidateDownload(IRequest request, IVirtualFile file)
    {
        var session = request.SessionAs<CustomUserSession>();
        var principal = request.GetClaimsPrincipal();
        
        // Admin role has access to all files.
        if (principal.IsInRole(RoleNames.Admin))
            return;

        var userFileAccess = GetFileAccessAndUserIdFromPath(file.VirtualPath);
        if (userFileAccess == null)
        {
            var logger = request.Resolve<ILoggerFactory>().CreateLogger<AppHost>();
            logger.LogWarning("Invalid file download path request: {PathInfo}", request.PathInfo);
            throw HttpError.NotFound("File not found.");
        }

        switch (userFileAccess.AccessType)
        {
            case FileAccessType.Private:
                if (userFileAccess.UserAuthId != null && session.UserAuthId == userFileAccess.UserAuthId)
                    return;
                throw HttpError.Forbidden("File is private to user.");
            case FileAccessType.Team:
                if (session.IsAuthenticated)
                    return;
                throw HttpError.Unauthorized("File download requires authentication.");
            case FileAccessType.Public:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public record UserFileAccess(string? UserAuthId, FileAccessType AccessType);

    private static UserFileAccess? GetFileAccessAndUserIdFromPath(string virtualFilePath)
    {
        var segments = virtualFilePath.Split("/");
        if (segments.Length < 3)
            return null;
        var access = segments[1];
        var firstSeg = segments[2];
        var accessType = access.ToEnum<FileAccessType>();
        return new UserFileAccess(accessType == FileAccessType.Private ? firstSeg : null, accessType);
    }
}
