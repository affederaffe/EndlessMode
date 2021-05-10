using System.Reflection;

using EndlessMode.Configuration;
using EndlessMode.Installers;

using HarmonyLib;

using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;

using SiraUtil.Zenject;


namespace EndlessMode
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private readonly Harmony _harmony = new("com.affederaffe.endlessmode");

        [Init]
        public Plugin(Logger logger, Config config, Zenjector zenjector)
        {
            zenjector.OnApp<EndlessAppInstaller>().WithParameters(logger, config.Generated<PluginConfig>());
            zenjector.OnMenu<EndlessMenuInstaller>();
            zenjector.OnGame<EndlessGameInstaller>().OnlyForStandard();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchAll("com.affederaffe.endlessmode");
        }
    }
}