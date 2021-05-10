using EndlessMode.UI;

using SiraUtil;

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