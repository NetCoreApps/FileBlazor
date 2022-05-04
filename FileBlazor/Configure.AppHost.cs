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
    public AppHost() : base("FileBlazor", typeof(MyServices).Assembly)
    {
    }

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig { });

        Plugins.Add(new CorsFeature(allowedHeaders: "Content-Type,Authorization",
            allowOriginWhitelist: new[] {
                "http://localhost:5000",
                "https://localhost:5001",
                "https://" + Environment.GetEnvironmentVariable("DEPLOY_CDN")
            }, allowCredentials: true));

        var appFs = new FileSystemVirtualFiles(ContentRootDirectory.RealPath.CombineWith("App_Data").AssertDir());
        var awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ??
                             Environment.GetEnvironmentVariable("LOCAL_AWS_ACCESS_KEY_ID");
        var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ??
                                 Environment.GetEnvironmentVariable("LOCAL_AWS_SECRET_ACCESS_KEY");
        var azureBlobConnString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING") ??
                                  Environment.GetEnvironmentVariable("LOCAL_AZURE_BLOB_CONNECTION_STRING");

        var s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.USEast1);
        var s3DataVfs = new S3VirtualFiles(s3Client, "file-blazor-demo");
        var azureBlobVfs = new AzureBlobVirtualFiles(azureBlobConnString, "file-blazor-demo");

        Plugins.Add(new FilesUploadFeature(
            new UploadLocation("azure", azureBlobVfs,
                readAccessRole: RoleNames.AllowAnon, resolvePath: ResolveUploadPath,
                validateUpload: ValidateUpload, validateDownload: ValidateDownload),
            new UploadLocation("s3", s3DataVfs,
                readAccessRole: RoleNames.AllowAnon, resolvePath: ResolveUploadPath,
                validateUpload: ValidateUpload, validateDownload: ValidateDownload),
            new UploadLocation("fs", appFs,
                readAccessRole: RoleNames.AllowAnon, resolvePath: ResolveUploadPath,
                validateUpload: ValidateUpload, validateDownload: ValidateDownload)
        ));

        static string ResolveUploadPath(FilesUploadContext ctx) =>
            ctx.Dto is IAppFile { FileAccessType: { } } createFile
                ? createFile.FileAccessType != FileAccessType.Private
                    ? ctx.GetLocationPath($"/{createFile.FileAccessType}/{ctx.FileName}")
                    : ctx.GetLocationPath($"/{createFile.FileAccessType}/{ctx.UserAuthId}/{ctx.FileName}")
                : throw HttpError.BadRequest("Invalid file creation request.");

        static void ValidateUpload(IRequest request, IHttpFile file)
        {
            if (request.Dto is IAppFile createFile)
            {
                var accessType = createFile.FileAccessType;
                var ext = file.FileName.LastRightPart('.');
                if (accessType == FileAccessType.Gallery && ext != null && FileExt.Images.Contains(ext) == false)
                    throw new ArgumentException("Supported file extensions: {0}".LocalizeFmt(request, string.Join(", ",
                        FileExt.Images.Select(x => '.' + x).ToList().OrderBy(x => x))), file.FileName);
            }
            else
                throw new HttpError("Invalid request.");
        }
    }

    private static void ValidateDownload(IRequest request, IVirtualFile file)
    {
        var session = request.GetSession();
        // Admin role has access to all files.
        if (session.HasRole(RoleNames.Admin, request.Resolve<IAuthRepository>()))
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
                throw HttpError.NotFound("File not found.");
            case FileAccessType.Gallery:
                if (session.IsAuthenticated)
                    return;
                throw HttpError.NotFound("File not found.");
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

    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
            services.ConfigureNonBreakingSameSiteCookies(context.HostingEnvironment));
}