using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformNonceStore : IOpenPlatformNonceStore
{
    private readonly IDistributedCache _distributedCache;

    public OpenPlatformNonceStore(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<bool> TryUseAsync(string clientId, string nonce, TimeSpan ttl)
    {
        var key = $"openplatform:nonce:{clientId}:{nonce}";
        var existing = await _distributedCache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(existing))
        {
            return false;
        }

        await _distributedCache.SetStringAsync(
            key,
            "1",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });

        return true;
    }
}
