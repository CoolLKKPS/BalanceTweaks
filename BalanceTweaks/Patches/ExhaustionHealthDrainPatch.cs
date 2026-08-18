using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    internal static class ExhaustionHealthDrainPatch
    {
        private const float DrainInterval = 0.5f;

        private const int DrainAmount = 2;

        private static readonly ConditionalWeakTable<PlayerControllerB, DrainState> drainStates = new ConditionalWeakTable<PlayerControllerB, DrainState>();

        private sealed class DrainState
        {
            public float timer;
        }

        [HarmonyPostfix]
        private static void Postfix(PlayerControllerB __instance)
        {
            if (!BalanceTweaksPlugin.EnableExhaustionHealthDrain.Value)
                return;

            if (!__instance.IsOwner)
                return;

            if (__instance != GameNetworkManager.Instance.localPlayerController)
                return;

            DrainState state = drainStates.GetOrCreateValue(__instance);

            float effectiveMeter = WalkingStaminaPatch.GetEffectiveSprintMeter(__instance);

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
