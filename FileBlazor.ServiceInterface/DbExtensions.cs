using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FileBlazor.ServiceModel;
using Microsoft.Extensions.Hosting;
using ServiceStack;
using ServiceStack.OrmLite;

namespace FileBlazor.ServiceInterface;

public static class DbExtensions
{
    public static async Task<UserProfile> GetUserProfileAsync(this IDbConnection db, int userId) => 
        await db.SingleAsync<UserProfile>(db.From<AppUser>().Where(x => x.Id == userId));

    public static async Task<UserResult> GetUserResultAsync(this IDbConnection db, int userId)
    {
        var userInfo = await db.SingleAsync<(string refId, string handle, string avatar, string profileUrl, int id)>(db.From<AppUser>()
            .Where(x => x.Id == userId).Select(x => new { x.ProfileUrl }));

        return new UserResult
        {
            Id = userInfo.id,
            RefId = userInfo.refId,
            Handle = userInfo.handle,
            Avatar = userInfo.avatar,
            ProfileUrl = userInfo.profileUrl,
        };
    }
}
