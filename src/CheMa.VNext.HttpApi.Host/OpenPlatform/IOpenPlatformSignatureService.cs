using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CheMa.VNext.OpenPlatform;

public interface IOpenPlatformSignatureService
{
    Task<OpenPlatformValidationResult> ValidateAsync(HttpContext httpContext);
}
