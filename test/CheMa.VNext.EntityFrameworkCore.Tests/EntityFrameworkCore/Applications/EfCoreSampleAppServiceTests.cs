using CheMa.VNext.Samples;
using Xunit;

namespace CheMa.VNext.EntityFrameworkCore.Applications;

[Collection(VNextTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<VNextEntityFrameworkCoreTestModule>
{

}
