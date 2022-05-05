using System.Collections.Generic;
using FileBlazor.ServiceModel.Types;
using ServiceStack;

namespace FileBlazor.ServiceModel;


public class QueryAppUser : QueryDb<AppUser>
{

}

public class QueryAppUserFile : QueryDb<SharedFsFile>
{
    
}

public class QueryAppUserS3File : QueryDb<SharedS3File>
{
    
}

public class QueryAppUserAzureFile : QueryDb<SharedAzureFile>
{
    
}

[AutoPopulate(nameof(SharedFsFile.AppUserId), Eval = "userAuthId")]
public class CreateFileSystemFile : ICreateDb<SharedFsFile>, IReturn<SharedFsFile>, ISharedFile
{
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("fs")]
    public FsFile AppFile { get; set; }
}

[AutoPopulate(nameof(SharedFsFile.AppUserId), Eval = "userAuthId")]
public class UpdateFileSystemFile : IUpdateDb<SharedFsFile>, IReturn<SharedFsFile>, ISharedFile
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("fs")]
    public FsFile AppFile { get; set; }
}

[AutoPopulate(nameof(SharedS3File.AppUserId), Eval = "userAuthId")]
[Route("/upload-create-s3-file")]
public class CreateS3File : ICreateDb<SharedS3File>, IReturn<SharedS3File>, ISharedFile
{
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("s3")]
    public S3File AppFile { get; set; }
}

[AutoPopulate(nameof(SharedS3File.AppUserId), Eval = "userAuthId")]
public class UpdateS3File : IUpdateDb<SharedS3File>, IReturn<SharedS3File>, ISharedFile
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("s3")]
    public S3File AppFile { get; set; }
}

[AutoPopulate(nameof(SharedAzureFile.AppUserId), Eval = "userAuthId")]
public class CreateAzureFile : ICreateDb<SharedAzureFile>, IReturn<SharedAzureFile>, ISharedFile
{
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("azure")]
    public AzureFile AppFile { get; set; }
}

[AutoPopulate(nameof(SharedAzureFile.AppUserId), Eval = "userAuthId")]
public class UpdateAzureFile : IUpdateDb<SharedAzureFile>, IReturn<SharedAzureFile>, ISharedFile
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    [Input(Type = "file"), UploadTo("azure")]
    public AzureFile AppFile { get; set; }
}