using ServiceStack;
using FileBlazor.ServiceModel;
using System;
using FileBlazor.ServiceModel.Types;
using ServiceStack.FluentValidation;
using ServiceStack.Web;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;

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
        var q = Db.From<TItemTable>().Join<TItemTable, TFileTable>((x, y) => x.Id == y.SharedFileId);
        var session = GetSession();
        var userAuthId = session.UserAuthId;
        var userAuth = AuthRepository.GetUserAuth(userAuthId);
        var userRoles = AuthRepository.GetRoles(userAuth);

        if (session.HasRole(RoleNames.Admin, AuthRepository))
            q.Where(x => x.FileAccessType == request.FileAccessType);
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
}

