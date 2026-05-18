using Microsoft.AspNetCore.Http;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformRequestContextAccessor : IOpenPlatformRequestContextAccessor
{
    private const string HttpContextItemKey = "__OpenPlatformRequestContext";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OpenPlatformRequestContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public OpenPlatformRequestContext? Current
    {
        get => _httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextItemKey, out var value) == true
            ? value as OpenPlatformRequestContext
            : null;
        set
        {
            if (_httpContextAccessor.HttpContext is null)
            {
                return;
            }

            _httpContextAccessor.HttpContext.Items[HttpContextItemKey] = value!;
        }
    }
}
