using CheMa.VNext.Samples;
using Xunit;

namespace CheMa.VNext.EntityFrameworkCore.Domains;

[Collection(VNextTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<VNextEntityFrameworkCoreTestModule>
{

}
