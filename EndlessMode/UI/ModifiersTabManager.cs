using System;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;

using Zenject;


namespace EndlessMode.UI
{
    public sealed class ModifiersTabManager : IInitializable, IDisposable
    {
        private readonly ModifiersTabHost _modifiersTabHost;

        public ModifiersTabManager(ModifiersTabHost modifiersTabHost)
        {
            _modifiersTabHost = modifiersTabHost;
        }

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("Endless Mode", "EndlessMode.Views.Modifiers.bsml", _modifiersTabHost, MenuType.Solo);
        }

        public void Dispose()
        {
            if (GameplaySetup.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
                GameplaySetup.instance.RemoveTab("Endless Mode");
        }
    }
}