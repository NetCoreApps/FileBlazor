using ServiceStack;
using ServiceStack.Web;
using ServiceStack.Data;
using ServiceStack.Html;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using FileBlazor.Client;
using FileBlazor.ServiceModel.Types;

[assembly: HostingStartup(typeof(FileBlazor.ConfigureAuthRepository))]

namespace FileBlazor;

public class AppUserAuthEvents : AuthEvents
{
    public override async Task OnAuthenticatedAsync(IRequest httpReq, IAuthSession session, IServiceBase authService,
        IAuthTokens tokens, Dictionary<string, string> authInfo, CancellationToken token = default)
    {
        var authRepo = HostContext.AppHost.GetAuthRepositoryAsync(httpReq);
        using (authRepo as IDisposable)
        {
            var userAuth = (AppUser)await authRepo.GetUserAuthAsync(session.UserAuthId, token);
            userAuth.ProfileUrl = session.GetProfileUrl();
            userAuth.LastLoginIp = httpReq.UserHostAddress;
            userAuth.LastLoginDate = DateTime.UtcNow;
            await authRepo.SaveUserAuthAsync(userAuth, token);
        }
    }
}

public class ConfigureAuthRepository : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services => services.AddSingleton<IAuthRepository>(c =>
            new OrmLiteAuthRepository<AppUser, UserAuthDetails>(c.Resolve<IDbConnectionFactory>()) {
                UseDistinctRoleTables = true
            }))
        .ConfigureAppHost(appHost => {
            var authRepo = appHost.Resolve<IAuthRepository>();
            authRepo.InitSchema();
            CreateUser(authRepo, "admin@email.com", "Admin User", "p@55wOrd", roles: new[] { RoleNames.Admin });
            CreateUser(authRepo, "manager@email.com", "The Manager", "p@55wOrd", roles: new[] { AppRoles.Employee, AppRoles.Manager });
            CreateUser(authRepo, "employee@email.com", "A Employee", "p@55wOrd", roles: new[] { AppRoles.Employee });

            // Removing unused UserName in Admin Users UI 
            appHost.Plugins.Add(new ServiceStack.Admin.AdminUsersFeature {
                
                // Show custom fields in Search Results
                QueryUserAuthProperties = new() {
                    nameof(AppUser.Id),
                    nameof(AppUser.Email),
                    nameof(AppUser.DisplayName),
                    nameof(AppUser.CreatedDate),
                    nameof(AppUser.LastLoginDate),
                },

                QueryMediaRules = new()
                {
                    MediaRules.ExtraSmall.Show<AppUser>(x => new { x.Id, x.Email, x.DisplayName }),
                    MediaRules.Small.Show<AppUser>(x => x.DisplayName),
                },

                // Add Custom Fields to Create/Edit User Forms
                UserFormLayout = new() {
                    new()
                    {
                        Input.For<AppUser>(x => x.Email),
                    },
                    new()
                    {
                        Input.For<AppUser>(x => x.DisplayName),
                    },
                    new()
                    {
                        Input.For<AppUser>(x => x.Company)
                    },
                    new() {
                        Input.For<AppUser>(x => x.PhoneNumber, c => c.Type = Input.Types.Tel)
                    },
                    new() {
                        Input.For<AppUser>(x => x.Nickname, c => {
                            c.Help = "Public alias (3-12 lower alpha numeric chars)";
                            c.Pattern = "^[a-z][a-z0-9_.-]{3,12}$";
                            //c.Required = true;
                        })
                    },
                    new() {
                        Input.For<AppUser>(x => x.ProfileUrl, c => c.Type = Input.Types.Url)
                    },
                    new() {
                        Input.For<AppUser>(x => x.IsArchived), Input.For<AppUser>(x => x.ArchivedDate),
                    },
                }
            });

        },
        afterConfigure: appHost => {
            appHost.AssertPlugin<AuthFeature>().AuthEvents.Add(new AppUserAuthEvents());
        });

    // Add initial Users to the configured Auth Repository
    public void CreateUser(IAuthRepository authRepo, string email, string name, string password, string[] roles)
    {
        if (authRepo.GetUserAuthByUserName(email) == null)
        {
            var newAdmin = new AppUser { Email = email, DisplayName = name };
            var user = authRepo.CreateUserAuth(newAdmin, password);
            authRepo.AssignRoles(user, roles);
        }
    }
}