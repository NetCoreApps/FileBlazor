using Microsoft.AspNetCore.Identity;
using ServiceStack;
using ServiceStack.DataAnnotations;

namespace FileBlazor.ServiceModel;

// Add profile data for application users by adding properties to the AppUser class
[Alias("AspNetUsers")]
public class AppUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    [Format(FormatMethods.IconRounded)]
    [Input(Type = "file"), UploadTo("users")]
    public string? ProfileUrl { get; set; }
    

    public DateTime? ArchivedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIp { get; set; }
}

[Alias("AspNetRoles")]
public class AppRole : IdentityRole<int>
{
    public AppRole() {}
    public AppRole(string roleName) : base(roleName) {}
}