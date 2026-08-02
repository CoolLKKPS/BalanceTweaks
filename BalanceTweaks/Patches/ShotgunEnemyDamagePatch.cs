using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

[HarmonyPatch(typeof(ShotgunItem), "ShootGun")]
internal static class ShotgunEnemyDamagePatch
{
    const int DamageClose = 3;  // < 3.7m
    const int DamageMedium = 2;  // 3.7m ~ 6m
    const int DamageFar = 1;  // > 6m

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        int remainingMatches = 0;

        foreach (CodeInstruction ci in instructions)
        {
            if (ci.opcode == OpCodes.Ldc_R4 && Mathf.Approximately((float)ci.operand, 3.7f))
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
                else if (ci.opcode == OpCodes.Ldc_I4 && ci.operand is int v && (v == 5 || v == 3 || v == 2))
                {
                    ci.operand = v switch
                    {
                        5 => DamageClose,
                        3 => DamageMedium,
                        _ => DamageFar,
                    };
                    remainingMatches--;
                }
            }
            yield return ci;
        }
    }
}
