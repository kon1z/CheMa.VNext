using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.Permissions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Volo.Abp.Authorization.Permissions;

namespace CheMa.VNext.Blazor.Client.Pages;

public partial class OpenPlatformSignatureDebug
{
    [Inject]
    protected IPermissionChecker PermissionChecker { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected static string[] HttpMethods { get; } = ["GET", "POST", "PUT", "DELETE", "PATCH"];

    protected OpenPlatformSignatureDebugInput Input { get; set; } = new()
    {
        Method = "POST",
        Path = "/api/open/orders",
        SignVersion = "v1"
    };

    protected string Signature { get; set; } = string.Empty;

    protected bool IsGenerating { get; set; }

    protected bool HasSignatureDebugPermission { get; set; }

    private const string PolicyName = OpenPlatformPermissions.SignatureDebug.Default;

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateTask;
        if (!authenticationState.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        HasSignatureDebugPermission = await PermissionChecker.IsGrantedAsync(OpenPlatformPermissions.SignatureDebug.Default);
    }

    protected void OnClientIdInput(ChangeEventArgs args) => Input.ClientId = args.Value?.ToString() ?? string.Empty;

    protected void OnTimestampInput(ChangeEventArgs args) => Input.Timestamp = args.Value?.ToString() ?? string.Empty;

    protected void OnNonceInput(ChangeEventArgs args) => Input.Nonce = args.Value?.ToString() ?? string.Empty;

    protected void OnSignVersionInput(ChangeEventArgs args) => Input.SignVersion = args.Value?.ToString();

    protected void OnMethodChanged(ChangeEventArgs args) => Input.Method = args.Value?.ToString() ?? "GET";

    protected void OnPathInput(ChangeEventArgs args) => Input.Path = args.Value?.ToString() ?? string.Empty;

    protected void OnQueryInput(ChangeEventArgs args) => Input.Query = args.Value?.ToString();

    protected void OnBodyInput(ChangeEventArgs args) => Input.Body = args.Value?.ToString();

    protected void OnAppSecretInput(ChangeEventArgs args) => Input.AppSecret = args.Value?.ToString() ?? string.Empty;

    protected async Task GenerateAsync()
    {
        if (!HasSignatureDebugPermission)
        {
            await UiMessageService.Warn(L["OpenPlatform:SignatureDebugPermissionDenied"]);
            return;
        }

        try
        {
            IsGenerating = true;
            Signature = string.Empty;
            var result = await SignatureDebugAppService.GenerateAsync(Input);
            Signature = result.Signature;
        }
        catch (System.Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
        finally
        {
            IsGenerating = false;
        }
    }

    protected Task FillExampleAsync()
    {
        Input = new OpenPlatformSignatureDebugInput
        {
            ClientId = "demo-client",
            Timestamp = "1710000000",
            Nonce = "debug-nonce-001",
            SignVersion = "v1",
            Method = "POST",
            Path = "/api/open/orders",
            Query = "page=1&size=20",
            Body = "{\"orderNo\":\"SO-1001\",\"amount\":128.5}",
            AppSecret = "demo-secret"
        };
        Signature = string.Empty;
        return Task.CompletedTask;
    }
}
