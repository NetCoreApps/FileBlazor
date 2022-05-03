﻿using ServiceStack.Auth;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace FileBlazor.ServiceModel.Types
{

    public class AppUserFsFile
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        public FileAccessType? FileAccessType { get; set; }
        public string? FilePath { get; set; }
        public int AppUserId { get; set; }
    }
    
    public class AppUserS3File
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        public FileAccessType? FileAccessType { get; set; }
        public string? FilePath { get; set; }
        public int AppUserId { get; set; }
    }

    public class AppUserAzureFile
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        public FileAccessType? FileAccessType { get; set; }
        public string? FilePath { get; set; }
        public int AppUserId { get; set; }
    }
    
    public enum FileAccessType
    {
        Private,
        Gallery,
        Public
    }

    public interface IAppFile
    {
        public FileAccessType? FileAccessType { get; set; }
        public string? FilePath { get; set; }
        public int AppUserId { get; set; }
    }

    // Custom User Table with extended Metadata properties
    public class AppUser : IUserAuth
    {
        public string? ProfileUrl { get; set; }
        public string? LastLoginIp { get; set; }

        public bool IsArchived { get; set; }
        public DateTime? ArchivedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }
        public int Id { get; set; }
        public string PrimaryEmail { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
        public string DigestHa1Hash { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Permissions { get; set; }
        public int? RefId { get; set; }
        public string RefIdStr { get; set; }
        public int InvalidLoginAttempts { get; set; }
        public DateTime? LastLoginAttempt { get; set; }
        public DateTime? LockedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BirthDateRaw { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Culture { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Language { get; set; }
        public string MailAddress { get; set; }
        public string Nickname { get; set; }
        public string PostalCode { get; set; }
        public string TimeZone { get; set; }
        public Dictionary<string, string> Meta { get; set; }
    }
}