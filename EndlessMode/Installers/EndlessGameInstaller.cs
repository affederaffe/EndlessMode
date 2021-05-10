using EndlessMode.Configuration;

using Zenject;


namespace EndlessMode.Installers
{
    public class EndlessGameInstaller : Installer
    {
        private readonly PluginConfig _config;

        public EndlessGameInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            if (_config.Enabled)
                Container.BindInterfacesTo<EndlessSongController>().AsSingle();
        }
    }
}