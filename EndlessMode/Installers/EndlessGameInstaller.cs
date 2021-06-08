using System;

using Zenject;


namespace EndlessMode.Installers
{
    public class EndlessGameInstaller : Installer
    {
        private readonly GameSongController _gameSongController;

        public EndlessGameInstaller(GameSongController gameSongController)
        {
            _gameSongController = gameSongController;
        }

        public override void InstallBindings()
        {
            Container.Unbind<GameSongController>();
            Container.Bind(typeof(GameSongController), typeof(IInitializable), typeof(IDisposable)).To<EndlessSongController>().FromNewComponentOn(_gameSongController.gameObject).AsSingle();
            _gameSongController.enabled = false;
        }
    }
}