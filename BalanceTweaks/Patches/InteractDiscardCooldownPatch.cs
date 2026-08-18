using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    internal static class InteractDiscardCooldownPatch
    {
        private const float KnifeCooldownTime = 0.43f;

        private static readonly FieldInfo knifeCooldownField = AccessTools.Field(typeof(KnifeItem), "timeAtLastDamageDealt");

        internal static bool IsInCooldown(PlayerControllerB player)
        {
            GrabbableObject heldObject = player.currentlyHeldObjectServer;
            if (heldObject == null)
            {
                return false;
            }
            Shovel shovel = heldObject as Shovel;
            if (shovel != null && shovel.reelingUp)
            {
                return true;
            }
            KnifeItem knife = heldObject as KnifeItem;
            if (knife != null && Time.realtimeSinceStartup - (float)knifeCooldownField.GetValue(knife) < KnifeCooldownTime)
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "Interact_performed")]
    internal static class BlockInteractPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(PlayerControllerB __instance)
        {
            if (!BalanceTweaksPlugin.EnableInteractDiscardBlock.Value)
            {
                return true;
            }
            return !InteractDiscardCooldownPatch.IsInCooldown(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "Discard_performed")]
    internal static class BlockDiscardPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(PlayerControllerB __instance)
        {
            if (!BalanceTweaksPlugin.EnableInteractDiscardBlock.Value)
            {
                return true;
            }
            return !InteractDiscardCooldownPatch.IsInCooldown(__instance);
        }
    }
}
