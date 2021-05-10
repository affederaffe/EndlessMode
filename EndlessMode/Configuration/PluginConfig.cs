using System.Runtime.CompilerServices;

using IPA.Config.Stores;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace EndlessMode.Configuration
{
    public class PluginConfig
    {
        public virtual bool Enabled { get; set; }
    }
}