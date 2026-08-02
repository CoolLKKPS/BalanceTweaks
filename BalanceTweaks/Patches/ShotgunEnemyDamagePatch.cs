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
        bool pastDistanceCheck = false;

        foreach (CodeInstruction ci in instructions)
        {
            if (ci.opcode == OpCodes.Ldc_R4 && Mathf.Approximately((float)ci.operand, 3.7f))
                pastDistanceCheck = true;

            if (pastDistanceCheck)
            {
                if (ci.opcode == OpCodes.Ldc_I4_5)
                {
                    ci.opcode = OpCodes.Ldc_I4;
                    ci.operand = DamageClose;
                    pastDistanceCheck = false;
                }
                else if (ci.opcode == OpCodes.Ldc_I4_3)
                {
                    ci.opcode = OpCodes.Ldc_I4;
                    ci.operand = DamageMedium;
                    pastDistanceCheck = false;
                }
                else if (ci.opcode == OpCodes.Ldc_I4_2)
                {
                    ci.opcode = OpCodes.Ldc_I4;
                    ci.operand = DamageFar;
                    pastDistanceCheck = false;
                }
                else if (ci.opcode == OpCodes.Ldc_I4 && ci.operand is int v && (v == 5 || v == 3 || v == 2))
                {
                    ci.operand = v switch
                    {
                        5 => DamageClose,
                        3 => DamageMedium,
                        _ => DamageFar,
                    };
                    pastDistanceCheck = false;
                }
            }
            yield return ci;
        }
    }
}
