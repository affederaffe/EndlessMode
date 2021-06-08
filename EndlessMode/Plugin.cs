using EndlessMode.Configuration;
using EndlessMode.Installers;

using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;

using SiraUtil.Zenject;


namespace EndlessMode
{
    [Plugin(RuntimeOptions.DynamicInit)]
    // ReSharper disable once UnusedType.Global
    public class Plugin
    {
        [Init]
        public Plugin(Logger logger, Config config, Zenjector zenjector)
        {
            zenjector.OnApp<EndlessAppInstaller>().WithParameters(logger, config.Generated<PluginConfig>());
            zenjector.OnMenu<EndlessMenuInstaller>();
            zenjector.OnGame<EndlessGameInstaller>().OnlyForStandard();
        }
    }
}