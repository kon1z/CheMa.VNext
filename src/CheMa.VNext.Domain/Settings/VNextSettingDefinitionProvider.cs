using Volo.Abp.Settings;

namespace CheMa.VNext.Settings;

public class VNextSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(VNextSettings.MySetting1));
    }
}
