using HarmonyLib;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(JesterAI), "Update")]
    internal static class JesterSpeedPatch
    {
        private static readonly AccessTools.FieldRef<JesterAI, bool> inKillAnimation = AccessTools.FieldRefAccess<JesterAI, bool>("inKillAnimation");

        [HarmonyPostfix]
        private static void Postfix(JesterAI __instance)
        {
            if (__instance.isEnemyDead)
                return;

            if (__instance.currentBehaviourStateIndex != 2)
                return;

            if (__instance.IsOwner)
                return;

            if (inKillAnimation(__instance) || __instance.stunNormalizedTimer > 0f)
                __instance.agent.speed = 0f;
            else
                __instance.agent.speed = Mathf.Clamp(__instance.agent.speed + (Time.deltaTime * 1.45f), 0f, 18f);
        }
    }
}
