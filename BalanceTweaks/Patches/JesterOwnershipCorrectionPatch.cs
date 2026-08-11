using HarmonyLib;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(RoundManager), "Update")]
    internal static class JesterOwnershipCorrectionPatch
    {
        private static float _timer;

        [HarmonyPostfix]
        private static void Postfix()
        {
            if (!BalanceTweaksPlugin.EnableJesterOwnershipCorrection.Value)
                return;

            _timer += Time.deltaTime;
            if (_timer < 0.8f)
                return;
            _timer = 0f;

            var enemies = RoundManager.Instance.SpawnedEnemies;
            if (enemies == null)
                return;

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.isEnemyDead)
                    continue;
                if (!(enemy is JesterAI jester))
                    continue;
                if (jester.currentBehaviourStateIndex != 2)
                    continue;
                if (!jester.IsOwner)
                    continue;

                var target = jester.targetPlayer;
                if (target == null)
                    continue;
                if (!jester.PlayerIsTargetable(target, false, false, true))
                    continue;
                if (target == GameNetworkManager.Instance.localPlayerController)
                    continue;

                jester.ChangeOwnershipOfEnemy(target.actualClientId);
            }
        }
    }
}
