using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace CheMa.VNext.OpenPlatform;

public class OpenAppManager : DomainService
{
    private readonly IRepository<OpenApp, Guid> _openAppRepository;

    public OpenAppManager(IRepository<OpenApp, Guid> openAppRepository)
    {
        _openAppRepository = openAppRepository;
    }

    public async Task<OpenAppSecretInfo> CreateAsync(
        string name,
        DateTime? beginTime = null,
        DateTime? endTime = null,
        string? allowedIpRanges = null,
        string? description = null)
    {
        var clientId = await GenerateClientIdAsync();
        var plainSecret = GenerateAppSecret();
        var cipherText = ProtectSecret(plainSecret);
        var app = new OpenApp(
            GuidGenerator.Create(),
            name,
            clientId,
            cipherText,
            MaskSecret(plainSecret),
            beginTime,
            endTime,
            allowedIpRanges,
            description);

        await _openAppRepository.InsertAsync(app, autoSave: true);

        return new OpenAppSecretInfo(app, plainSecret);
    }

    public async Task<OpenAppSecretInfo> ResetSecretAsync(OpenApp app)
    {
        var plainSecret = GenerateAppSecret();
        app.SetSecret(ProtectSecret(plainSecret), MaskSecret(plainSecret));
        await _openAppRepository.UpdateAsync(app, autoSave: true);
        return new OpenAppSecretInfo(app, plainSecret);
    }

    public string UnprotectSecret(OpenApp app)
    {
        var fullCipherBytes = Convert.FromBase64String(app.AppSecretCipherText);
        using var stream = new MemoryStream(fullCipherBytes);
        var iv = new byte[16];
        stream.ReadExactly(iv);
        var cipherBytes = new byte[stream.Length - stream.Position];
        stream.ReadExactly(cipherBytes);

        using var aes = Aes.Create();
        aes.Key = DeriveKey();
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private async Task<string> GenerateClientIdAsync()
    {
        while (true)
        {
            var clientId = $"op_{Guid.NewGuid():N}";
            if (clientId.Length > OpenPlatformConsts.MaxClientIdLength)
            {
                clientId = clientId[..OpenPlatformConsts.MaxClientIdLength];
            }

            if (!await _openAppRepository.AnyAsync(x => x.ClientId == clientId))
            {
                return clientId;
            }
        }
    }

    private static string GenerateAppSecret()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private static string ProtectSecret(string plainSecret)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainSecret);

        using var aes = Aes.Create();
        aes.Key = DeriveKey();
        aes.GenerateIV();
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        using var stream = new MemoryStream();
        stream.Write(aes.IV);
        stream.Write(cipherBytes);
        return Convert.ToBase64String(stream.ToArray());
    }

    private static string MaskSecret(string plainSecret)
    {
        if (plainSecret.Length <= 8)
        {
            return plainSecret;
        }

        return $"{plainSecret[..4]}****{plainSecret[^4..]}";
    }

    private static byte[] DeriveKey()
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes("CheMa.VNext.OpenPlatform.SecretKey.v1"));
    }
}

public sealed record OpenAppSecretInfo(OpenApp OpenApp, string PlainSecret);
