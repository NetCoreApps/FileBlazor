using FileBlazor.ServiceModel;
using ServiceStack;
using ServiceStack.Blazor;
using Microsoft.AspNetCore.Components;

namespace FileBlazor.Client.UI;

public class UserState
{
    public const int InitialTake = 50;
    public const int NextPage = 100;
    public const int StaticTake = 500;
    public const int StaticPagedTake = 250;

    public string? PrerenderedUrl { get; set; }
    public string? PrerenderedHtml { get; set; }

    public void SetPrerenderedHtml(string url, string html)
    {
        PrerenderedUrl = url;
        PrerenderedHtml = html;
    }

    public string? GetPrerenderedHtml(string url)
    {
        var matches = url == PrerenderedUrl && !string.IsNullOrEmpty(PrerenderedHtml);
        log("GetPrerenderedHtml(): {0} == {1} = {2}", url, PrerenderedUrl ?? "", matches);
        if (matches)
            return PrerenderedHtml;
        return null;
    }

    public void RemovePrerenderedHtml()
    {
        PrerenderedUrl = PrerenderedHtml = null;
        OnRemovePrerenderedHtml?.Invoke();
    }

    public Action? OnRemovePrerenderedHtml { get; set; }

    public CachedLocalStorage LocalStorage { get; }
    public IServiceGateway Client { get; }
    
    // Capture images that should have loaded in Browsers cache
    public HashSet<int> HasIntersected { get; } = new();

    public string? GetAvatar() => User?.Avatar;

    public string? RefId => User?.RefId;
    public UserResult User { get; set; }
    public List<string> Roles { get; set; } = new();
    
    public bool IsLoading { get; set; }

    NavigationManager NavigationManager { get; }

    public UserState(CachedLocalStorage localStorage, IClientFactory clientFactory, NavigationManager navigationManager)
    {
        LocalStorage = localStorage;
        Client = clientFactory.GetGateway();
        NavigationManager = navigationManager;
    }

    async Task<ApiResult<TResponse>> ApiAsync<TResponse>(IReturn<TResponse> request)
    {
        IsLoading = true;
        NotifyStateChanged();

        var api = await Client.ManagedApiAsync(request);

        IsLoading = false;
        NotifyStateChanged();

        return api;
    }

    async Task<ApiResult<EmptyResponse>> ApiAsync(IReturnVoid request)
    {
        IsLoading = true;
        NotifyStateChanged();

        var api = await Client.ManagedApiAsync(request);

        IsLoading = false;
        NotifyStateChanged();

        return api;
    }

    protected virtual async Task OnApiErrorAsync(object requestDto, IHasErrorStatus apiError)
    {
        if (BlazorConfig.Instance.OnApiErrorAsync != null)
            await BlazorConfig.Instance.OnApiErrorAsync(requestDto, apiError);
    }

    void log(string message, params object[] args) => BlazorConfig.Instance.GetLog()?.LogDebug(message, args);
    

    public event Action? OnChange;
    private void NotifyStateChanged()
    {
        if (OnChange != null)
        {
            BlazorUtils.LogDebug("UserState NotifyStateChanged()");
            OnChange.Invoke();
        }
    }
}