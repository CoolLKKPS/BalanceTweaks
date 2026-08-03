using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace BalanceTweaksPlugin
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class BalanceTweaksPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> CreateNetworkPrefab;
        public static ConfigEntry<bool> ShotgunConductive;
        public static ConfigEntry<bool> ShotgunEnemyDamage;
        public static ConfigEntry<bool> BeltBagShotgunAmmo;
        public static ConfigEntry<bool> AmmoWeight;
        public static ConfigEntry<bool> ShotgunWeight;
        public static ConfigEntry<bool> ShovelWeight;
        public static ConfigEntry<bool> KnifeWeight;
        public static ConfigEntry<bool> EnableShovelNarrowHitbox;
        public static ConfigEntry<bool> EnableShovelTriggerHit;
        public static ConfigEntry<bool> EnableShovelLinecastBlock;
        public static ConfigEntry<bool> EnableKnifeTriggerHit;
        public static ConfigEntry<bool> EnableKnifeLinecastBlock;

        private void Awake()
        {
            Instance = this;
            logger = base.Logger;

            CreateNetworkPrefab = Config.Bind("General", "CreateNetworkPrefab", false, "");
            ShotgunConductive = Config.Bind("General", "ShotgunConductive", true, "");
            ShotgunEnemyDamage = Config.Bind("General", "ShotgunEnemyDamage", true, "");
            BeltBagShotgunAmmo = Config.Bind("General", "BeltBagShotgunAmmo", true, "");
            AmmoWeight = Config.Bind("Balance", "AmmoWeight", false, "");
            ShotgunWeight = Config.Bind("Balance", "ShotgunWeight", true, "");
            ShovelWeight = Config.Bind("Balance", "ShovelWeight", true, "");
            KnifeWeight = Config.Bind("Balance", "KnifeWeight", true, "");
            EnableShovelNarrowHitbox = Config.Bind("HitDetection", "EnableShovelNarrowHitbox", true, "");
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
