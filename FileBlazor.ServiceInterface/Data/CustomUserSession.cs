using System.Security.Claims;
using FileBlazor.ServiceModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ServiceStack;
using ServiceStack.Web;

namespace FileBlazor.Data;

// Add any additional metadata properties you want to store in the Users Typed Session
public class CustomUserSession : AuthUserSession
{
    public string? Handle { get; set; }
    public string? Avatar { get; set; }

    public int GetUserId() => UserAuthId.ToInt();
    
    public override void PopulateFromClaims(IRequest httpReq, ClaimsPrincipal principal)
    {
        Handle = principal.FindFirstValue(JwtClaimTypes.NickName);
        Avatar = principal.FindFirstValue(JwtClaimTypes.Picture);
    }
}

public class AdditionalUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<AppUser, AppRole>
{
    public AdditionalUserClaimsPrincipalFactory(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    public override async Task<ClaimsPrincipal> CreateAsync(AppUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        var claims = new List<Claim>();

        identity.AddClaims(claims);
        return principal;
    }
}
