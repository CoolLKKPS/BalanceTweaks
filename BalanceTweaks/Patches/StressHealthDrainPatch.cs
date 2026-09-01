using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    internal static class StressHealthDrainPatch
    {
        private const float DrainInterval = 0.5f;

        private const int DrainAmount = 5;

        private static readonly ConditionalWeakTable<PlayerControllerB, DrainState> drainStates = new ConditionalWeakTable<PlayerControllerB, DrainState>();

        private sealed class DrainState
        {
            public float timer;
        }

        [HarmonyPostfix]
        private static void Postfix(PlayerControllerB __instance)
        {
            if (!BalanceTweaksPlugin.EnableStressMechanism.Value || !BalanceTweaksPlugin.EnableStressHealthDrain.Value)
                return;

            if (__instance != GameNetworkManager.Instance.localPlayerController)
                return;

            DrainState state = drainStates.GetOrCreateValue(__instance);

            float effectiveMeter = StressMechanismPatch.GetEffectiveSprintMeter(__instance);

            if (effectiveMeter > 0f || __instance.isPlayerDead)
            {
                state.timer = 0f;
                return;
            }

            state.timer += Time.deltaTime;
            if (state.timer < DrainInterval)
                return;

            state.timer = 0f;
            __instance.DamagePlayer(DrainAmount, true, true, CauseOfDeath.Suffocation, 0, false, default);
        }
    }
}
