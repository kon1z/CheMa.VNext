using System;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace CheMa.VNext.OpenPlatform;

public class OpenAppManagerTests : VNextDomainTestBase<VNextDomainTestModule>
{
    private readonly OpenAppManager _openAppManager;
    private readonly IRepository<OpenApp, Guid> _openAppRepository;

    public OpenAppManagerTests()
    {
        _openAppManager = GetRequiredService<OpenAppManager>();
        _openAppRepository = GetRequiredService<IRepository<OpenApp, Guid>>();
    }

    [Fact]
    public async Task Should_Create_Open_App_With_Usable_Secret()
    {
        OpenAppSecretInfo result = default!;

        await WithUnitOfWorkAsync(async () =>
        {
            result = await _openAppManager.CreateAsync("test-app");
        });

        result.OpenApp.ClientId.ShouldStartWith("op_");
        result.PlainSecret.ShouldNotBeNullOrWhiteSpace();

        await WithUnitOfWorkAsync(async () =>
        {
            var entity = await _openAppRepository.GetAsync(result.OpenApp.Id);
            _openAppManager.UnprotectSecret(entity).ShouldBe(result.PlainSecret);
        });
    }
}
