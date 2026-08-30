using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    internal static class StressMechanismPatch
    {
        private static readonly AccessTools.FieldRef<PlayerControllerB, bool> isWalking = AccessTools.FieldRefAccess<PlayerControllerB, bool>("isWalking");
        private const float HealthFactorStartHp = 100f;
        private const float HealthFactorEndHp = 20f;
        private const float HealthFactorMax = 1.25f;
        internal static float stressTimer;
        internal static float stressChargeThreshold;
        internal static float SecondsToFullStress;
        internal static float pendingDamageTaken;
        internal static int otherTotal;
        internal static int otherAlive;
        internal static float alivePercent;

        [HarmonyPostfix]
        private static void Postfix(PlayerControllerB __instance)
        {
            __instance.sprintMeter = GetEffectiveSprintMeter(__instance);
        }

        internal static float GetEffectiveSprintMeter(PlayerControllerB player)
        {
            if (!BalanceTweaksPlugin.EnableStressMechanism.Value)
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

            float walkDrainMultiplier = stressTimer * 0.3f;

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

                StressMechanismPatch.pendingDamageTaken += damageNumber;
            }
        }

        private static void UpdateStressTimer(PlayerControllerB player)
        {
            if (player.isPlayerDead)
                return;

            otherTotal = StartOfRound.Instance.connectedPlayersAmount;
            otherAlive = StartOfRound.Instance.livingPlayers - (player.isPlayerDead ? 0 : 1);
            alivePercent = otherTotal > 0 ? Mathf.Clamp01((float)otherAlive / otherTotal) : 0f;
            float fearMultiplier;
            float damageMultiplier;

            if (otherTotal == 0)
            {
                stressChargeThreshold = player.maxInsanityLevel * 0.02f;
                SecondsToFullStress = 950f;
                fearMultiplier = 0.0014f;
                damageMultiplier = 0.004f;
            }
            else
            {
                stressChargeThreshold = player.maxInsanityLevel * 0.04f;
                SecondsToFullStress = Mathf.Lerp(792f, 1188f, alivePercent);
                fearMultiplier = Mathf.Lerp(0.002f, 0.0012f, alivePercent);
                damageMultiplier = Mathf.Lerp(0.0048f, 0.0032f, alivePercent);
            }

            if (StartOfRound.Instance.inShipPhase)
            {
                stressTimer = 0f;
                pendingDamageTaken = 0f;
                return;
            }

            float healthFactor = Mathf.Lerp(1f, HealthFactorMax, Mathf.InverseLerp(HealthFactorStartHp, HealthFactorEndHp, player.health));

            if (player.insanityLevel > stressChargeThreshold)
            {
                // (insanityLevel / (50 - stressChargeThreshold)) / SecondsToFullStress
                float chargePerSecond = Mathf.InverseLerp(stressChargeThreshold, player.maxInsanityLevel, player.insanityLevel) / SecondsToFullStress;
                stressTimer += Time.deltaTime * chargePerSecond * healthFactor;
            }

            if (StartOfRound.Instance.fearLevel > 0f)
            {
                stressTimer += Time.deltaTime * StartOfRound.Instance.fearLevel * fearMultiplier * healthFactor;
            }

            if (pendingDamageTaken > 0f)
            {
                stressTimer += pendingDamageTaken * damageMultiplier * healthFactor;
                pendingDamageTaken = 0f;
            }

            stressTimer = Mathf.Clamp(stressTimer, 0f, 1f);
        }
    }
}
