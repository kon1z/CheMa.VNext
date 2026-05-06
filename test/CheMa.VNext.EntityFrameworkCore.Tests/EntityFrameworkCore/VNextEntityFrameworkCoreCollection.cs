using Xunit;

namespace CheMa.VNext.EntityFrameworkCore;

[CollectionDefinition(VNextTestConsts.CollectionDefinitionName)]
public class VNextEntityFrameworkCoreCollection : ICollectionFixture<VNextEntityFrameworkCoreFixture>
{

}
