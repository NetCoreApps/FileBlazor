﻿using System.Collections.Generic;
using FileBlazor.ServiceModel.Types;
using ServiceStack;

namespace FileBlazor.ServiceModel;


[ValidateIsAdmin]
public class QueryAppUser : QueryDb<AppUser>
{

}

[AutoFilter(QueryTerm.Ensure, nameof(FileAccessType), Value = FileAccessType.Public)]
[Tag("Files")]
public class QueryPublicFileSystemFileItems : QueryDb<FileSystemFileItem>
{
    public FileAccessType? FileAccessTypes { get; set; }
}

[ValidateIsAuthenticated]
[Tag("Files")]
public class QueryFileSystemFileItems : QueryDb<FileSystemFileItem>, IQueryFileItem
{
    public int? AppUserId { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    public string? FileName { get; set; }
}

[AutoFilter(QueryTerm.Ensure, nameof(FileAccessType), Value = FileAccessType.Public)]
[Tag("Files")]
public class QueryPublicS3FileItems : QueryDb<S3FileItem>
{
    public FileAccessType? FileAccessTypes { get; set; }
}

public interface IQueryFileItem
{
    public int? AppUserId { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    public string? FileName { get; set; }
}

[ValidateIsAuthenticated]
[Tag("Files")]
public class QueryS3FileItems : QueryDb<S3FileItem>, IQueryFileItem
{
    public int? AppUserId { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    public string? FileName { get; set; }
}

[AutoFilter(QueryTerm.Ensure, nameof(FileAccessType), Value = FileAccessType.Public)]
[Tag("Files")]
public class QueryPublicAzureFileItems : QueryDb<AzureFileItem>
{
    public FileAccessType? FileAccessTypes { get; set; }
}

[ValidateIsAuthenticated]
[Tag("Files")]
public class QueryAzureFileItems : QueryDb<AzureFileItem>, IQueryFileItem
{
    public int? AppUserId { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    public string? FileName { get; set; }
}

[AutoPopulate(nameof(FileSystemFileItem.AppUserId), Eval = "userAuthId")]
[Tag("Files")]
public class CreateFileSystemFileItem : ICreateDb<FileSystemFileItem>, IReturn<FileSystemFileItem>, IFileItemRequest
{
    public FileAccessType? FileAccessType { get; set; }
    public string? RoleName { get; set; }
    [Input(Type = "file"), UploadTo("fs")]
    public FileSystemFile AppFile { get; set; }
}

[AutoPopulate(nameof(FileSystemFileItem.AppUserId), Eval = "userAuthId")]
[Tag("Files")]
public class UpdateFileSystemFileItem : IUpdateDb<FileSystemFileItem>, IReturn<FileSystemFileItem>, IFileItemRequest
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    public string? RoleName { get; set; }
    [Input(Type = "file"), UploadTo("fs")]
    public FileSystemFile AppFile { get; set; }
}

[AutoPopulate(nameof(S3FileItem.AppUserId), Eval = "userAuthId")]
[Route("/upload-create-s3-file")]
[Tag("Files")]
public class CreateS3FileItem : ICreateDb<S3FileItem>, IReturn<S3FileItem>, IFileItemRequest
{
    public FileAccessType? FileAccessType { get; set; }
    public string? RoleName { get; set; }
    [Input(Type = "file"), UploadTo("s3")]
    public S3File AppFile { get; set; }
}

[AutoPopulate(nameof(S3FileItem.AppUserId), Eval = "userAuthId")]
[Tag("Files")]
public class UpdateS3FileItem : IUpdateDb<S3FileItem>, IReturn<S3FileItem>, IFileItemRequest
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    public string? RoleName { get; set; }
    [Input(Type = "file"), UploadTo("s3")]
    public S3File AppFile { get; set; }
}

[AutoPopulate(nameof(AzureFileItem.AppUserId), Eval = "userAuthId")]
[Tag("Files")]
public class CreateAzureFileItem : ICreateDb<AzureFileItem>, IReturn<AzureFileItem>, IFileItemRequest
{
    public FileAccessType? FileAccessType { get; set; }
    public string? RoleName { get; set; }
    [Input(Type = "file"), UploadTo("azure")]
    public AzureFile AppFile { get; set; }
}

[AutoPopulate(nameof(AzureFileItem.AppUserId), Eval = "userAuthId")]
[Tag("Files")]
public class UpdateAzureFileItem : IUpdateDb<AzureFileItem>, IReturn<AzureFileItem>, IFileItemRequest
{
    public int Id { get; set; }
    public FileAccessType? FileAccessType { get; set; }
    public string? RoleName { get; set; }
    [Input(Type = "file"), UploadTo("azure")]
    public AzureFile AppFile { get; set; }
}

[ValidateIsAuthenticated]
[Tag("Files")]
public class DeleteFileSystemFileItem : IDeleteDb<FileSystemFileItem>, IReturnVoid
{
    public int Id { get; set; }
}

[ValidateIsAuthenticated]
[Tag("Files")]
public class DeleteS3FileItem : IDeleteDb<S3FileItem>, IReturnVoid
{
    public int Id { get; set; }
}

[ValidateIsAuthenticated]
[Tag("Files")]
public class DeleteAzureFileItem : IDeleteDb<AzureFileItem>, IReturnVoid
{
    public int Id { get; set; }
}