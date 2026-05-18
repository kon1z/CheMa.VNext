using System;
using System.Threading.Tasks;

namespace CheMa.VNext.OpenPlatform;

public interface IOpenPlatformNonceStore
{
    Task<bool> TryUseAsync(string clientId, string nonce, TimeSpan ttl);
}
