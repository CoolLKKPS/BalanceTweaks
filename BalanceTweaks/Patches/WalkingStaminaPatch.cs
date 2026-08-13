using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    internal static class WalkingStaminaPatch
    {
        private static readonly AccessTools.FieldRef<PlayerControllerB, bool> isWalking = AccessTools.FieldRefAccess<PlayerControllerB, bool>("isWalking");

        [HarmonyPostfix]
        private static void Postfix(PlayerControllerB __instance)
        {
            if (!BalanceTweaksPlugin.EnableWalkDrainsStamina.Value)
                return;

            if (!__instance.IsOwner)
                return;

            if (__instance.isSprinting)
                return;

            if (__instance.isMovementHindered > 0)
                return;

            if (!isWalking(__instance))
                return;

            float drunknessMultiplier = 1f;
            if (__instance.drunkness > 0.02f)
            {
                drunknessMultiplier *= Mathf.Abs(StartOfRound.Instance.drunknessSpeedEffect.Evaluate(__instance.drunkness) - 1.25f);
            }

            float deltaTime = Time.deltaTime;
            float sprintTime = __instance.sprintTime;
            float carryWeight = __instance.carryWeight;

            float walkDrainMultiplier = __instance.isCrouching ? 0f : 0.3f;
            float vanillaRegenAmount = deltaTime / (sprintTime + 9f) * drunknessMultiplier;
            float drainAmount = deltaTime / sprintTime * carryWeight * drunknessMultiplier * walkDrainMultiplier;

            // First delete all vanilla regen, then apply the drain amount.
            __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - vanillaRegenAmount - drainAmount, 0f, 1f);
        }
    }
}
