using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace BalanceTweaksPlugin
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class BalanceTweaksPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Instance = this;
            logger = base.Logger;
            harmony.PatchAll();
            Logger.LogInfo("BalanceTweaks is loaded!");
        }

        public const string PLUGIN_GUID = "BalanceTweaks";
        public const string PLUGIN_NAME = "BalanceTweaks";
        public const string PLUGIN_VERSION = "1.0.0";
        public const string PLUGIN_VERSION_FULL = PLUGIN_VERSION + ".0";

        Harmony harmony = new Harmony(PLUGIN_GUID);

        public static ManualLogSource logger;
        public static BalanceTweaksPlugin Instance;
    }
}
