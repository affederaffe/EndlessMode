using EndlessMode.UI;

using Zenject;


namespace EndlessMode.Installers
{
    public class EndlessMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<ModifiersTabHost>().AsSingle();
            Container.BindInterfacesTo<ModifiersTabManager>().AsSingle();
        }
    }
}