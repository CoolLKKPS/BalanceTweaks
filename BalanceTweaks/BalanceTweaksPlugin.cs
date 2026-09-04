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
        public static ConfigEntry<bool> ShotgunWeight;
        public static ConfigEntry<bool> ShovelWeight;
        public static ConfigEntry<bool> KnifeWeight;
        public static ConfigEntry<bool> EnableInteractDiscardBlock;
        public static ConfigEntry<bool> EnableShovelNarrowHitbox;
        public static ConfigEntry<bool> EnableShovelTriggerHit;
        public static ConfigEntry<bool> EnableShovelLinecastBlock;
        public static ConfigEntry<bool> EnableKnifeTriggerHit;
        public static ConfigEntry<bool> EnableKnifeLinecastBlock;
        public static ConfigEntry<bool> EnableJesterOwnershipCorrection;
        public static ConfigEntry<bool> EnableStressMechanism;
        public static ConfigEntry<bool> EnableStressHealthDrain;

        private void Awake()
        {
            Instance = this;
            logger = base.Logger;

            CreateNetworkPrefab = Config.Bind("General", "CreateNetworkPrefab", false, "Define whether to create a network prefab");
            ShotgunConductive = Config.Bind("General", "ShotgunConductive", true, "Define whether the shotgun is conductive");
            ShotgunEnemyDamage = Config.Bind("General", "ShotgunEnemyDamage", true, "Define whether to nerf shotgun damage");
            BeltBagShotgunAmmo = Config.Bind("General", "BeltBagShotgunAmmo", true, "Define whether the belt bag can't store shotgun ammo");
            ShotgunWeight = Config.Bind("Balance", "ShotgunWeight", true, "Define whether to modify shotgun weight");
            ShovelWeight = Config.Bind("Balance", "ShovelWeight", true, "Define whether to modify shovel weight");
            KnifeWeight = Config.Bind("Balance", "KnifeWeight", true, "Define whether to modify knife weight");
            EnableInteractDiscardBlock = Config.Bind("Balance", "EnableInteractDiscardBlock", false, "Define whether to block interacting and discarding during shovel reel-up or knife cooldown");
            EnableShovelNarrowHitbox = Config.Bind("HitDetection", "EnableShovelNarrowHitbox", true, "Define whether to narrow the shovel hit SphereCast radius");
            EnableShovelTriggerHit = Config.Bind("HitDetection", "EnableShovelTriggerHit", true, "Define whether to allow shovel to hit through trigger colliders");
            EnableShovelLinecastBlock = Config.Bind("HitDetection", "EnableShovelLinecastBlock", true, "Define whether to change shovel linecast queryTriggerInteraction");
            EnableKnifeTriggerHit = Config.Bind("HitDetection", "EnableKnifeTriggerHit", true, "Define whether to allow knife to hit through trigger colliders");
            EnableKnifeLinecastBlock = Config.Bind("HitDetection", "EnableKnifeLinecastBlock", true, "Define whether to change knife linecast queryTriggerInteraction");
            EnableJesterOwnershipCorrection = Config.Bind("Jester", "EnableJesterOwnershipCorrection", true, "Define whether to periodically correct Jester ownership");
            EnableStressMechanism = Config.Bind("Stress", "EnableStressMechanism", true, "Define whether to enable stress mechanism");
            EnableStressHealthDrain = Config.Bind("Stress", "EnableStressHealthDrain", true, "Define whether to enable stress health drain");

            harmony.PatchAll();
            Logger.LogInfo("LKKBalanceTweaks is loaded!");

            gameObject.AddComponent<Effects.AudioManager>();
            gameObject.AddComponent<Effects.StressVignetteEffect>();
            gameObject.AddComponent<Effects.DesaturateEffect>();
            gameObject.AddComponent<Effects.TinnitusEffect>();
#if DEBUG
            gameObject.AddComponent<DebugTools.StressTimerDebugUI>();
#endif
        }

        public const string PLUGIN_GUID = "LKKBalanceTweaks";
        public const string PLUGIN_NAME = "LKKBalanceTweaks";
        public const string PLUGIN_VERSION = "1.0.8";
        public const string PLUGIN_VERSION_FULL = PLUGIN_VERSION + ".0";

        private readonly Harmony harmony = new Harmony(PLUGIN_GUID);

        public static ManualLogSource logger;
        public static BalanceTweaksPlugin Instance;
    }
}
