using System;

using EndlessMode.Configuration;

using HarmonyLib;


namespace EndlessMode.HarmonyPatches
{
    [HarmonyPatch(typeof(SongController), nameof(SongController.SendSongDidFinishEvent))]
    internal static class SongControllerPatch
    {
        internal static PluginConfig? PluginConfig;
        internal static event Action? SongDidFinishEvent;

        public static bool Prefix()
        {
            if (!PluginConfig!.Enabled) return true;
            SongDidFinishEvent?.Invoke();
            return false;
        }
    }
}