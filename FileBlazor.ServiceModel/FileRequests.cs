using FileBlazor.ServiceModel.Types;
using ServiceStack;

namespace FileBlazor.ServiceModel;

public class QueryAppUserFile : QueryDb<AppUserFsFile>
{
    
}

public class QueryAppUserS3File : QueryDb<AppUserS3File>
{
    
}

public class QueryAppUserAzureFile : QueryDb<AppUserAzureFile>
{
    
}

public class CreateFileSystemFile : ICreateDb<AppUserFsFile>, IReturn<AppUserFsFile>, IAppFile
{
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("fs")]
    public string? FilePath { get; set; }
    [Ref(Model = nameof(AppUser), RefId = nameof(AppUser.Id), RefLabel = nameof(AppUser.DisplayName))]
    public int AppUserId { get; set; }
}

public class UpdateFileSystemFile : IUpdateDb<AppUserFsFile>, IReturn<AppUserFsFile>, IAppFile
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("fs")]
    public string? FilePath { get; set; }
    [Ref(Model = nameof(AppUser), RefId = nameof(AppUser.Id), RefLabel = nameof(AppUser.DisplayName))]
    public int AppUserId { get; set; }
}

public class CreateS3File : ICreateDb<AppUserS3File>, IReturn<AppUserS3File>, IAppFile
{
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("s3")]
    public string? FilePath { get; set; }
    [Ref(Model = nameof(AppUser), RefId = nameof(AppUser.Id), RefLabel = nameof(AppUser.DisplayName))]
    public int AppUserId { get; set; }
}

public class UpdateS3File : IUpdateDb<AppUserS3File>, IReturn<AppUserS3File>, IAppFile
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("s3")]
    public string? FilePath { get; set; }
    [Ref(Model = nameof(AppUser), RefId = nameof(AppUser.Id), RefLabel = nameof(AppUser.DisplayName))]
    public int AppUserId { get; set; }
}

public class CreateAzureFile : ICreateDb<AppUserAzureFile>, IReturn<AppUserAzureFile>, IAppFile
{
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("azure")]
    public string? FilePath { get; set; }
    [Ref(Model = nameof(AppUser), RefId = nameof(AppUser.Id), RefLabel = nameof(AppUser.DisplayName))]
    public int AppUserId { get; set; }
}

public class UpdateAzureFile : IUpdateDb<AppUserAzureFile>, IReturn<AppUserAzureFile>, IAppFile
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("azure")]
    public string? FilePath { get; set; }
    [Ref(Model = nameof(AppUser), RefId = nameof(AppUser.Id), RefLabel = nameof(AppUser.DisplayName))]
    public int AppUserId { get; set; }
}
