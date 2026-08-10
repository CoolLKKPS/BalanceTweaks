using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    // This patch doesn't mean to fix ownership issues, it just make sure host authority when host have ownership
    [HarmonyPatch(typeof(JesterAI), "OnCollideWithPlayer")]
    internal static class JesterCollisionPatch
    {
        private static readonly AccessTools.FieldRef<JesterAI, bool> inKillAnimation = AccessTools.FieldRefAccess<JesterAI, bool>("inKillAnimation");

        private static readonly MethodInfo isSeparatedByMineshaftElevator = AccessTools.Method(typeof(EnemyAI), "IsSeparatedByMineshaftElevator");

        [HarmonyPostfix]
        private static void Postfix(JesterAI __instance, Collider other)
        {
            if (__instance.currentBehaviourStateIndex != 2)
                return;
            if (!__instance.IsOwner)
                return;
            if (__instance.isEnemyDead)
                return;
            // Original code
            if (!__instance.ventAnimationFinished)
                return;

            PlayerControllerB player = other?.GetComponent<PlayerControllerB>();
            if (player == null)
                return;
            if (player != __instance.targetPlayer)
                return;
            if (inKillAnimation(__instance))
                return;
            if (__instance.stunNormalizedTimer >= 0f)
                return;
            if (!__instance.PlayerIsTargetable(player, false, false, true))
                return;
            // Original code
            if ((bool)isSeparatedByMineshaftElevator.Invoke(__instance, new object[] { player.transform.position }))
                return;

            __instance.KillPlayerServerRpc((int)player.playerClientId);
        }
    }
}
