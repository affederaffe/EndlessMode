using EndlessMode.Configuration;
using EndlessMode.HarmonyPatches;

using IPA.Logging;

using SiraUtil;

using Zenject;


namespace EndlessMode.Installers
{
    public class EndlessAppInstaller : Installer
    {
        private readonly Logger _logger;
        private readonly PluginConfig _config;

        public EndlessAppInstaller(Logger logger, PluginConfig config)
        {
            _logger = logger;
            _config = config;
            SongControllerPatch.PluginConfig = config;
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config).AsSingle();
        }
    }
}