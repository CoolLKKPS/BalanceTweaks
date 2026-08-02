using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

[HarmonyPatch(typeof(Shovel), "HitShovel")]
internal static class ShovelAttackPatch
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 0.8f)
            {
                instruction.operand = 0.75f;
            }
            else if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 1.5f)
            {
                instruction.operand = 1.5f;     // v45 is 1.85f
            }
            yield return instruction;
        }
    }
}
