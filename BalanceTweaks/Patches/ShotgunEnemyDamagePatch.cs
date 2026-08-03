using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BalanceTweaksPlugin
{
    [HarmonyPatch(typeof(ShotgunItem), "ShootGun")]
    internal static class ShotgunEnemyDamagePatch
    {
        const float OriginalRangeThreshold = 3.7f;          // Verify
        const int OriginalDamageClose = 5;                  // < RangeThreshold
        const int OriginalDamageMedium = 3;                 // RangeThreshold ~ 6m
        const int OriginalDamageFar = 2;                    // > 6m

        const int DamageClose = 4;
        const int DamageMedium = 3;
        const int DamageFar = 2;

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
}
