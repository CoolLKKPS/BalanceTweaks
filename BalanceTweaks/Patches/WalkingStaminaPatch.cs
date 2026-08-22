using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    internal static class WalkingStaminaPatch
    {
        private static readonly AccessTools.FieldRef<PlayerControllerB, bool> isWalking = AccessTools.FieldRefAccess<PlayerControllerB, bool>("isWalking");
        internal static float stressTimer;
        internal static float stressChargeThreshold;
        internal static float SecondsToFullStress;
        internal static float pendingDamageTaken;

        [HarmonyPostfix]
        private static void Postfix(PlayerControllerB __instance)
        {
            __instance.sprintMeter = GetEffectiveSprintMeter(__instance);
        }

        internal static float GetEffectiveSprintMeter(PlayerControllerB player)
        {
            if (!BalanceTweaksPlugin.EnableWalkDrainsStamina.Value)
                return player.sprintMeter;

            if (player != GameNetworkManager.Instance.localPlayerController)
                return player.sprintMeter;

            UpdateStressTimer(player);

            if (player.insanityLevel <= 0f)
                return player.sprintMeter;

            if (player.isSprinting)
                return player.sprintMeter;

            if (player.isCrouching)
                return player.sprintMeter;

            if (player.isMovementHindered > 0)
                return player.sprintMeter;

            if (!isWalking(player))
                return player.sprintMeter;

            float drunknessMultiplier = 1f;
            if (player.drunkness > 0.02f)
            {
                drunknessMultiplier *= Mathf.Abs(StartOfRound.Instance.drunknessSpeedEffect.Evaluate(player.drunkness) - 1.25f);
            }

            float deltaTime = Time.deltaTime;

            float walkDrainMultiplier = stressTimer * 0.25f;

            // Vanilla things
            float vanillaRegenAmount = deltaTime / (player.sprintTime + 9f) * drunknessMultiplier;
            float drainAmount = deltaTime / player.sprintTime * player.carryWeight * drunknessMultiplier * walkDrainMultiplier;

            float regenFactor = Mathf.Clamp01(stressTimer / 0.5f);
            float drainFactor = Mathf.InverseLerp(0.5f, 1f, stressTimer);
            float effectiveDrain = drainAmount * drainFactor;

            // The result
            return Mathf.Clamp(player.sprintMeter - (vanillaRegenAmount * regenFactor) - effectiveDrain, 0f, 1f);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
        internal static class DamageTakenStressPatch
        {
            [HarmonyPostfix]
            private static void Postfix(PlayerControllerB __instance, int damageNumber)
            {
                if (__instance != GameNetworkManager.Instance.localPlayerController)
                {
                    return;
                }

                WalkingStaminaPatch.pendingDamageTaken += damageNumber;
            }
        }

        private static void UpdateStressTimer(PlayerControllerB player)
        {
            // 50 x 0.02 = 1, the base we start stress things
            stressChargeThreshold = player.maxInsanityLevel * 0.02f;

            if (StartOfRound.Instance.connectedPlayersAmount == 0)
            {
                SecondsToFullStress = 270f;
            }
            else
            {
                int otherTotal = StartOfRound.Instance.connectedPlayersAmount;
                int otherAlive = StartOfRound.Instance.livingPlayers - (player.isPlayerDead ? 0 : 1);
                float alivePercent = otherTotal > 0 ? (float)otherAlive / otherTotal : 0f;
                SecondsToFullStress = Mathf.Lerp(210f, 540f, Mathf.Clamp01(alivePercent));
            }

            if (StartOfRound.Instance.inShipPhase)
            {
                stressTimer = 0f;
                pendingDamageTaken = 0f;
                return;
            }

            if (player.insanityLevel > stressChargeThreshold)
            {
                // (insanityLevel / (50 - stressChargeThreshold)) / SecondsToFullStress
                float chargePerSecond = Mathf.InverseLerp(stressChargeThreshold, player.maxInsanityLevel, player.insanityLevel) / SecondsToFullStress;
                // No frame related
                stressTimer += Time.deltaTime * chargePerSecond;
            }

            if (StartOfRound.Instance.fearLevel > 0f)
            {
                stressTimer += Time.deltaTime * StartOfRound.Instance.fearLevel * 0.001f;
            }

            if (pendingDamageTaken > 0f)
            {
                stressTimer += pendingDamageTaken * 0.002f;
                pendingDamageTaken = 0f;
            }

            stressTimer = Mathf.Clamp(stressTimer, 0f, 1f);
        }
    }
}
