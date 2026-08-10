using HarmonyLib;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(FlashlightItem), "DiscardItem")]
    internal static class FlashlightDiscardItemPatch
    {
        [HarmonyPrefix]
        private static void Prefix(FlashlightItem __instance)
        {
            if (__instance.isBeingUsed
                && __instance.insertedBattery != null
                && !__instance.insertedBattery.empty
                && (int)(__instance.insertedBattery.charge * 100f) == 0)
            {
                __instance.UseUpBatteries();
            }
        }
    }
}
