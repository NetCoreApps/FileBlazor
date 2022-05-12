using ServiceStack.Auth;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using System.Runtime.Serialization;

namespace FileBlazor.ServiceModel.Types
{
    public class S3File : IFile
    {
        [AutoIncrement] public int Id { get; set; }
        
        public string FileName { get; set; }

        [Format(FormatMethods.Attachment)] 
        public string FilePath { get; set; }
        public string ContentType { get; set; }

        [Format(FormatMethods.Bytes)] 
        public long ContentLength { get; set; }

        [References(typeof(S3FileItem))] 
        public int SharedFileId { get; set; }
    }

    public class FileSystemFile : IFile
    {
        [AutoIncrement] 
        public int Id { get; set; }
        
        public string FileName { get; set; }

        [Format(FormatMethods.Attachment)] 
        public string FilePath { get; set; }
        public string ContentType { get; set; }

        [Format(FormatMethods.Bytes)] 
        public long ContentLength { get; set; }

        [References(typeof(FileSystemFileItem))] 
        public int SharedFileId { get; set; }
    }

    public class AzureFile : IFile
    {
        [AutoIncrement] public int Id { get; set; }

        public string FileName { get; set; }

        [Format(FormatMethods.Attachment)] 
        public string FilePath { get; set; }
        public string ContentType { get; set; }

        [Format(FormatMethods.Bytes)] 
        public long ContentLength { get; set; }

        [References(typeof(AzureFileItem))] 
        public int SharedFileId { get; set; }
    }

    public record FileItemWithFile(IFileItem FileItem, IFile File);

    public class FileSystemFileItem : IFileItem
    {
        [AutoIncrement] 
        public int Id { get; set; }

        public FileAccessType? FileAccessType { get; set; }

        [Reference] 
        public FileSystemFile AppFile { get; set; }


        [References(typeof(AppUser))]
        public int AppUserId { get; set; }

        [Reference]
        public AppUser User { get; set; }

        [Index]
        public string RoleName { get; set; }
    }

    public class S3FileItem : IFileItem
    {
        [AutoIncrement] 
        public int Id { get; set; }

        public FileAccessType? FileAccessType { get; set; }

        [Reference] 
        public S3File AppFile { get; set; }
        [References(typeof(AppUser))]
        public int AppUserId { get; set; }

        [Reference]
        public AppUser User { get; set; }

        [Index]
        public string RoleName { get; set; }
    }

    public class AzureFileItem : IFileItem
    {
        [AutoIncrement] 
        public int Id { get; set; }

        public FileAccessType? FileAccessType { get; set; }

        [Reference] 
        public AzureFile AppFile { get; set; }
        [References(typeof(AppUser))]
        public int AppUserId { get; set; }

        [Reference]
        public AppUser User { get; set; }

        [Index]
        public string RoleName { get; set; }
    }

    public enum FileAccessType
    {
        /// <summary>
        /// Example of custom download access control.
        /// User files can only be downloaded by the user who uploaded them or those with the Admin role.
        /// User files can be listed by anyone.
        /// </summary>
        Private,
        /// <summary>
        /// Example of custom upload control by file type.
        /// Files must have a valid web image extension.
        /// </summary>
        Team,
        /// <summary>
        /// Example of public shared files.
        /// Files can be on any type, shared with anonymous users but only uploaded by authenticated users.
        /// </summary>
        Public
    }

    public interface IFileItem
    {
        public FileAccessType? FileAccessType { get; set; }
        public string? RoleName { get; set; }
        public AppUser User { get; set; }
    }

    public interface IFileItemRequest
    {
        public FileAccessType? FileAccessType { get; set; }
        public string? RoleName { get; set; }
    }

    public interface IFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
    }


    // Custom User Table with extended Metadata properties
    public class AppUser : IUserAuth
    {
        [AutoIncrement] 
        public int Id { get; set; }
        public string DisplayName { get; set; }

        [Index]
        [Format(FormatMethods.LinkEmail)]
        public string Email { get; set; }

        // Custom Properties
        [Format(FormatMethods.IconRounded)]
        [Input(Type = "file"), UploadTo("users")]
        public string ProfileUrl { get; set; }

        public string Title { get; set; }
        public string JobArea { get; set; }
        public string Location { get; set; }
        public int Salary { get; set; }
        public string About { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string LastLoginIp { get; set; }

        // UserAuth Properties
        public string UserName { get; set; }
        public string PrimaryEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BirthDateRaw { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Culture { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Language { get; set; }
        public string MailAddress { get; set; }
        public string Nickname { get; set; }
        public string PostalCode { get; set; }
        public string TimeZone { get; set; }
        [IgnoreDataMember] 
        public string Salt { get; set; }
        [IgnoreDataMember] 
        public string PasswordHash { get; set; }
        [IgnoreDataMember] 
        public string DigestHa1Hash { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int InvalidLoginAttempts { get; set; }
        public DateTime? LastLoginAttempt { get; set; }
        public DateTime? LockedDate { get; set; }
        [IgnoreDataMember] 
        public string RecoveryToken { get; set; }

        //Custom Reference Data
        public int? RefId { get; set; }
        public string RefIdStr { get; set; }
        public Dictionary<string, string> Meta { get; set; }
    }
}