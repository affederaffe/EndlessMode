using BeatSaberMarkupLanguage.Attributes;

using EndlessMode.Configuration;


namespace EndlessMode.UI
{
    public class ModifiersTabHost
    {
        private readonly PluginConfig _config;

        public ModifiersTabHost(PluginConfig config)
        {
            _config = config;
        }

        [UIValue("enabled")]
        public bool Enabled
        {
            get => _config.Enabled;
            set => _config.Enabled = value;
        }
    }
}