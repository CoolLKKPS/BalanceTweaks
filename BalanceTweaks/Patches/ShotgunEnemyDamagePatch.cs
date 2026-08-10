using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(ShotgunItem), "ShootGun")]
    internal static class ShotgunEnemyDamagePatch
    {
        private const float OriginalRangeThreshold = 3.7f;      // Verify
        private const int OriginalDamageClose = 5;              // < RangeThreshold
        private const int OriginalDamageMedium = 3;             // RangeThreshold ~ 6m
        private const int OriginalDamageFar = 2;                // > 6m

        private const int DamageClose = 4;
        private const int DamageMedium = 3;
        private const int DamageFar = 2;

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int remainingMatches = 0;

            foreach (CodeInstruction ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_R4 && Mathf.Approximately((float)ci.operand, OriginalRangeThreshold))
                    remainingMatches = 3;

                if (remainingMatches > 0)
                {
                    if (ci.opcode == OpCodes.Ldc_I4_5)
                    {
                        ci.opcode = OpCodes.Ldc_I4;
                        ci.operand = DamageClose;
                        remainingMatches--;
                    }
                    else if (ci.opcode == OpCodes.Ldc_I4_3)
                    {
                        ci.opcode = OpCodes.Ldc_I4;
                        ci.operand = DamageMedium;
                        remainingMatches--;
                    }
                    else if (ci.opcode == OpCodes.Ldc_I4_2)
                    {
                        ci.opcode = OpCodes.Ldc_I4;
                        ci.operand = DamageFar;
                        remainingMatches--;
                    }
                    else if (ci.opcode == OpCodes.Ldc_I4 && ci.operand is int v
                        && (v == OriginalDamageClose || v == OriginalDamageMedium || v == OriginalDamageFar))
                    {
                        ci.operand = v == OriginalDamageClose ? DamageClose
                                   : v == OriginalDamageMedium ? DamageMedium
                                   : DamageFar;
                        remainingMatches--;
                    }
                }
                yield return ci;
            }
        }
    }

    [HarmonyPatch(typeof(EnemyAI), "HitEnemyServerRpc")]
    internal static class ShotgunEnemyDamageSyncPatch
    {
        private static readonly int[] OriginalDamage = { 5, 3, 2 };
        private static readonly int[] ModdedDamage = { 4, 3, 2 };

        [HarmonyPrefix]
        private static void SyncShotgunDamage(ref int force, int playerWhoHit)
        {
            if (!StartOfRound.Instance.IsHost)
                return;

            if (playerWhoHit < 0 || playerWhoHit >= StartOfRound.Instance.allPlayerScripts.Length)
                return;

            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerWhoHit];
            if (player == null || !(player.currentlyHeldObjectServer is ShotgunItem))
                return;

            int[] from = BalanceTweaksPlugin.ShotgunEnemyDamage.Value ? OriginalDamage : ModdedDamage;
            int[] to = BalanceTweaksPlugin.ShotgunEnemyDamage.Value ? ModdedDamage : OriginalDamage;

            for (int i = 0; i < from.Length; i++)
            {
                if (force == from[i])
                {
                    force = to[i];
                    break;
                }
            }
        }
    }
}
