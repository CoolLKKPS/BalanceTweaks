using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
    internal static class BlockScrollSwitchPatch
    {
        private const float KnifeCooldownTime = 0.43f;

        private static readonly FieldInfo knifeCooldownField =
            AccessTools.Field(typeof(KnifeItem), "timeAtLastDamageDealt");

        [HarmonyPrefix]
        private static bool Prefix(PlayerControllerB __instance)
        {
            Shovel shovel = __instance.currentlyHeldObjectServer as Shovel;
            if (shovel != null && shovel.reelingUp)
            {
                return false;
            }
            KnifeItem knife = __instance.currentlyHeldObjectServer as KnifeItem;
            if (knife != null && Time.realtimeSinceStartup - (float)knifeCooldownField.GetValue(knife) < KnifeCooldownTime)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "UseUtilitySlot_performed")]
    internal static class BlockUtilitySlotSwitchPatch
    {
        private const float KnifeCooldownTime = 0.43f;

        private static readonly FieldInfo knifeCooldownField =
            AccessTools.Field(typeof(KnifeItem), "timeAtLastDamageDealt");

        [HarmonyPrefix]
        private static bool Prefix(PlayerControllerB __instance)
        {
            Shovel shovel = __instance.currentlyHeldObjectServer as Shovel;
            if (shovel != null && shovel.reelingUp)
            {
                return false;
            }
            KnifeItem knife = __instance.currentlyHeldObjectServer as KnifeItem;
            if (knife != null && Time.realtimeSinceStartup - (float)knifeCooldownField.GetValue(knife) < KnifeCooldownTime)
            {
                return false;
            }
            return true;
        }
    }
}
