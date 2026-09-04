using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    internal static class StressMechanismPatch
    {
        private static readonly AccessTools.FieldRef<PlayerControllerB, bool> isWalking = AccessTools.FieldRefAccess<PlayerControllerB, bool>("isWalking");
        private const float HealthFactorStartHp = 100f;
        private const float HealthFactorEndHp = 20f;

        private const float HealthFactorMultiplier = 0.2f;
        private const float FearFactorMultiplier = 0.2f;

        private const float SoloShipRate = 0.000462f;
        private const float SoloOutsideRate = 0.000694f;
        private const float SoloFactoryRate = 0.000925f;

        private const float MultiShipRate = 0.000694f;
        private const float MultiOutsideRate = 0.001041f;
        private const float MultiFactoryRate = 0.001388f;

        private const float NearOthersRadius = 17f;
        private const float CompanionshipMultiplier = 0.5f;
        private const float PlayerFactorMultiplier = 1.15f;

        private static readonly HashSet<string> NoStressLevelSceneNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CompanyBuilding",
        };

        internal static float stressTimer;
        internal static float pendingDamageTaken;
        internal static int otherTotal;
        internal static int otherAlive;
        internal static float alivePercent;
        internal static float currentLocationRate;

        [HarmonyPostfix]
        private static void Postfix(PlayerControllerB __instance)
        {
            UpdateStress(__instance);
            __instance.sprintMeter = GetEffectiveSprintMeter(__instance);
        }

        internal static void UpdateStress(PlayerControllerB player)
        {
            if (!BalanceTweaksPlugin.EnableStressMechanism.Value)
                return;

            if (player != GameNetworkManager.Instance.localPlayerController)
                return;

            UpdateStressTimer(player);
        }

        internal static float GetEffectiveSprintMeter(PlayerControllerB player)
        {
            if (!BalanceTweaksPlugin.EnableStressMechanism.Value)
                return player.sprintMeter;

            if (player != GameNetworkManager.Instance.localPlayerController)
                return player.sprintMeter;

            if (!player.isInsideFactory)
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

        [HarmonyPatch(typeof(LungProp), "EquipItem")]
        internal static class ApparatusPulledPatch
        {
            private const float PullStressGain = 0.04f;

            [HarmonyPrefix]
            private static void Prefix(LungProp __instance)
            {
                if (!BalanceTweaksPlugin.EnableStressMechanism.Value)
                    return;

                if (!__instance.isLungDocked)
                    return;

                PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (localPlayer == null || localPlayer.isPlayerDead)
                    return;

                stressTimer = Mathf.Min(1f, stressTimer + PullStressGain);
            }
        }

        private static bool IsNoStressLevel()
        {
            return StartOfRound.Instance != null && StartOfRound.Instance.currentLevel != null && NoStressLevelSceneNames.Contains(StartOfRound.Instance.currentLevel.sceneName);
        }

        private static float GetLocationRate(PlayerControllerB player)
        {
            bool solo = otherTotal == 0;

            if (player.isInsideFactory)
                return solo ? SoloFactoryRate : MultiFactoryRate;

            if (player.isInHangarShipRoom)
                return solo ? SoloShipRate : MultiShipRate;

            return solo ? SoloOutsideRate : MultiOutsideRate;
        }

        internal static float GetHealthFactor(PlayerControllerB player)
        {
            return Mathf.Lerp(0f, HealthFactorMultiplier, Mathf.InverseLerp(HealthFactorStartHp, HealthFactorEndHp, player.health));
        }

        private static void UpdateStressTimer(PlayerControllerB player)
        {
            if (player.isPlayerDead)
                return;

            otherTotal = StartOfRound.Instance.connectedPlayersAmount;
            otherAlive = StartOfRound.Instance.livingPlayers - (player.isPlayerDead ? 0 : 1);
            alivePercent = otherTotal > 0 ? Mathf.Clamp01((float)otherAlive / otherTotal) : 0f;
            bool solo = otherTotal == 0;
            float damageMultiplier;

            if (solo)
            {
                damageMultiplier = 0.0005f;
            }
            else
            {
                damageMultiplier = Mathf.Lerp(0.00075f, 0.000375f, alivePercent);
            }

            if (StartOfRound.Instance.inShipPhase)
            {
                stressTimer = 0f;
                pendingDamageTaken = 0f;
                return;
            }

            if (IsNoStressLevel())
                return;

            float healthFactor = GetHealthFactor(player);
            float fearFactor = Mathf.Lerp(0f, FearFactorMultiplier, Mathf.Clamp01(StartOfRound.Instance.fearLevel));
            float playerFactor = solo ? 1f : Mathf.Lerp(PlayerFactorMultiplier, 1f, alivePercent);
            float companionshipMultiplier = player.NearOtherPlayers(NearOthersRadius) ? CompanionshipMultiplier : 1f;
            float locationRate = GetLocationRate(player) * (1f + healthFactor + fearFactor) * playerFactor * companionshipMultiplier;
            currentLocationRate = locationRate;
            stressTimer += Time.deltaTime * locationRate;

            if (pendingDamageTaken > 0f)
            {
                stressTimer += pendingDamageTaken * damageMultiplier * (1f + healthFactor);
                pendingDamageTaken = 0f;
            }

            stressTimer = Mathf.Clamp(stressTimer, 0f, 1f);
        }
    }
}
