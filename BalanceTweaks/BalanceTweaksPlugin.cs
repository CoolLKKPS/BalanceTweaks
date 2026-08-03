using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace BalanceTweaksPlugin
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class BalanceTweaksPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> EnableShovelTriggerHit;
        public static ConfigEntry<bool> EnableShovelLinecastBlock;
        public static ConfigEntry<bool> EnableKnifeTriggerHit;
        public static ConfigEntry<bool> EnableKnifeLinecastBlock;
        public static ConfigEntry<bool> CreateNetworkPrefab;

        private void Awake()
        {
            Instance = this;
            logger = base.Logger;

            CreateNetworkPrefab = Config.Bind("General", "CreateNetworkPrefab", false, "");
            EnableShovelTriggerHit = Config.Bind("HitDetection", "EnableShovelTriggerHit", true, "");
            EnableShovelLinecastBlock = Config.Bind("HitDetection", "EnableShovelLinecastBlock", true, "");
            EnableKnifeTriggerHit = Config.Bind("HitDetection", "EnableKnifeTriggerHit", true, "");
            EnableKnifeLinecastBlock = Config.Bind("HitDetection", "EnableKnifeLinecastBlock", true, "");

            harmony.PatchAll();
            Logger.LogInfo("BalanceTweaksForMe is loaded!");
        }

        public const string PLUGIN_GUID = "BalanceTweaksForMe";
        public const string PLUGIN_NAME = "BalanceTweaksForMe";
        public const string PLUGIN_VERSION = "1.0.1";
        public const string PLUGIN_VERSION_FULL = PLUGIN_VERSION + ".0";

        Harmony harmony = new Harmony(PLUGIN_GUID);

        public static ManualLogSource logger;
        public static BalanceTweaksPlugin Instance;
    }
}
