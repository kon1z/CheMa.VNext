using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CheMa.VNext.Localization;
using CheMa.VNext.MultiTenancy;
using Volo.Abp.Account.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;
using CheMa.VNext.Permissions;

namespace CheMa.VNext.Blazor.Client.Menus;

public class VNextMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public VNextMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<VNextResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                VNextMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home"
            )
        );

        var administration = context.Menu.GetAdministration();

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

        var openPlatform = new ApplicationMenuItem(
            VNextMenus.OpenPlatform,
            l["Menu:OpenPlatform"],
            icon: "fas fa-key"
        ).RequirePermissions(OpenPlatformPermissions.GroupName);

        openPlatform.AddItem(
            new ApplicationMenuItem(
                VNextMenus.OpenApps,
                l["Menu:OpenApps"],
                "/open-platform/apps",
                icon: "fas fa-plug"
            ).RequirePermissions(OpenPlatformPermissions.OpenApps.Default)
        );

        openPlatform.AddItem(
            new ApplicationMenuItem(
                VNextMenus.OpenPlatformSignatureDebug,
                l["Menu:OpenPlatformSignatureDebug"],
                "/open-platform/signature-debug",
                icon: "fas fa-signature"
            ).RequirePermissions(OpenPlatformPermissions.SignatureDebug.Default)
        );

        context.Menu.AddItem(openPlatform);

        return Task.CompletedTask;
    }

    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var accountStringLocalizer = context.GetLocalizer<AccountResource>();

        var authServerUrl = _configuration["AuthServer:Authority"] ?? "";

        context.Menu.AddItem(new ApplicationMenuItem(
            "Account.Manage",
            accountStringLocalizer["MyAccount"],
            $"{authServerUrl.EnsureEndsWith('/')}Account/Manage",
            icon: "fa fa-cog",
            order: 1000,
            target: "_blank").RequireAuthenticated());

        return Task.CompletedTask;
    }
}
