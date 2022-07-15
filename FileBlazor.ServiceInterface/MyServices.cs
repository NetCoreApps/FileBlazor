using ServiceStack;
using FileBlazor.ServiceModel;
using System;
using FileBlazor.ServiceModel.Types;
using ServiceStack.FluentValidation;
using ServiceStack.Web;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileBlazor.ServiceInterface;

public class MyServices : Service
{
    public static string AssertName(string Name) => Name.IsNullOrEmpty() 
        ? throw new ArgumentNullException(nameof(Name))
        : Name;

    public object Any(Hello request) =>
        new HelloResponse { Result = $"Hello, {AssertName(request.Name)}!" };

    public object Any(HelloSecure request) => 
        new HelloResponse { Result = $"Hello, {AssertName(request.Name)}!" };


    public IAutoQueryDb AutoQuery { get; set; }

    public object Get(QueryS3FileItems request)
    {
        return QueryFiles<QueryS3FileItems,S3FileItem,S3File>(request);
    }

    public object Get(QueryAzureFileItems request)
    {
        return QueryFiles<QueryAzureFileItems,AzureFileItem, AzureFile>(request);
    }

    public object Get(QueryFileSystemFileItems request)
    {
        return QueryFiles<QueryFileSystemFileItems, FileSystemFileItem, FileSystemFile>(request);
    }

    private QueryResponse<TItemTable> QueryFiles<TReq, TItemTable, TFileTable>(TReq request)
        where TReq : QueryDb<TItemTable>, IQueryFileItem
        where TItemTable : IFileItem
        where TFileTable : IFile
    {
        var q = AutoQuery.CreateQuery(request, base.Request, Db);
        var session = GetSession();
        var userAuthId = session.UserAuthId;
        var userAuth = AuthRepository.GetUserAuth(userAuthId);
        var userRoles = AuthRepository.GetRoles(userAuth);

        if (session.HasRole(RoleNames.Admin, AuthRepository))
        {
            if (request.FileAccessType != null)
                q.Where(x => x.FileAccessType == request.FileAccessType);
        }
        else
        {
            switch (request.FileAccessType)
            {
                case FileAccessType.Private:
                    q.Where(x => (x.FileAccessType == FileAccessType.Private && x.AppUserId == int.Parse(userAuthId)));
                    break;
                case FileAccessType.Team:
                    q.Where(x => Sql.In(x.RoleName, userRoles) && x.FileAccessType == FileAccessType.Team);
                    break;
                case FileAccessType.Public:
                    q.Where(x => x.FileAccessType == FileAccessType.Public);
                    break;
                case null:
                    q.Where(x => (x.FileAccessType == FileAccessType.Private && x.AppUserId == int.Parse(userAuthId)))
                        .Or(x => x.FileAccessType == FileAccessType.Public)
                        .Or(x => Sql.In(x.RoleName, userRoles));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var result = AutoQuery.Execute(request, q, Request);
        return result;
    }

    public async Task Delete(DeleteFileSystemFileItem request)
    {
        var fileItem = Db.LoadSingleById<FileSystemFileItem>(request.Id);
        var fileId = fileItem.AppFile.Id;
        await DeleteFile<FileSystemFileItem,FileSystemFile>(fileId, fileItem.Id);
    }

    public async Task Delete(DeleteS3FileItem request)
    {
        var fileItem = Db.LoadSingleById<S3FileItem>(request.Id);
        var fileId = fileItem.AppFile.Id;
        await DeleteFile<S3FileItem, S3File>(fileId, fileItem.Id);
    }

    public async Task Delete(DeleteAzureFileItem request)
    {
        var fileItem = Db.LoadSingleById<AzureFileItem>(request.Id);
        var fileId = fileItem.AppFile.Id;
        await DeleteFile<AzureFileItem, AzureFile>(fileId, fileItem.Id);
    }

    private async Task DeleteFile<TItemTable, TFileTable>(long fileId, long fileItemId)
        where TItemTable : IFileItem
        where TFileTable : IFile
    {
        using var transaction = Db.OpenTransaction();
        var file = Db.SingleById<TFileTable>(fileId);
        var fileItem = Db.SingleById<TItemTable>(fileItemId);
        using var deleteService = this.ResolveService<DeleteFileUploadService>();
        var locationName = deleteUploadLocationMapping[typeof(TFileTable)];
        await deleteService.Delete(new DeleteFileUpload
        {
            Name = locationName,
            Path = file.FilePath.Substring($"uploads/{locationName}/".Length)
        });
        Db.Delete(file);
        Db.Delete(fileItem);
        transaction.Commit();
    }

    private Dictionary<Type, string> deleteUploadLocationMapping = new()
    {
        { typeof(S3File), "s3" },
        { typeof(AzureFile), "azure" },
        { typeof(FileSystemFile), "fs" }
    };
}


