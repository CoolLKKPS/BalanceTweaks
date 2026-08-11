using HarmonyLib;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourState))]
    internal static class JesterStatePatch
    {
        [HarmonyPostfix]
        private static void Postfix(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != 2)
                return;

            if (!(__instance is JesterAI))
                return;

            if (!__instance.IsServer)
                return;

            if (!__instance.IsOwner)
                return;

            if (__instance.targetPlayer == null)
                return;

            if (__instance.targetPlayer == GameNetworkManager.Instance.localPlayerController)
                return;

            __instance.ChangeOwnershipOfEnemy(__instance.targetPlayer.actualClientId);
        }
    }
}
