using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Blazor;

namespace FileBlazor.Client;

/// <summary>
/// Manages App Authentication State
/// </summary>
public class ServiceStackStateProvider : BlazorWasmAuthenticationStateProvider
{
    public ServiceStackStateProvider(BlazorWasmAuthContext context, ILogger<ServiceStackStateProvider> log)
        : base(context, log) { }
}
