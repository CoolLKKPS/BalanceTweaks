using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BalanceTweaksPlugin
{
    [HarmonyPatch(typeof(Shovel), "HitShovel")]
    internal static class ShovelAttackPatch
    {
        const float OriginalSphereCastRadius = 0.8f;
        const float SphereCastRadius = 0.75f;

        const float OriginalSphereCastDistance = 1.5f;
        const float SphereCastDistance = 1.5f;

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == OriginalSphereCastRadius)
                {
                    instruction.operand = SphereCastRadius;
                }
                else if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == OriginalSphereCastDistance)
                {
                    instruction.operand = SphereCastDistance;
                }
                yield return instruction;
            }
        }
    }
}
