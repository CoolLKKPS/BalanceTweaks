using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(KnifeItem), "HitKnife")]
    internal static class KnifeAttackPatch
    {
        private const int QueryTriggerIgnore = 1;
        private const int QueryTriggerUseGlobal = 0;

        private const sbyte QueryTriggerIgnore_SByte = 1;

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction ci = codes[i];

                if (BalanceTweaksPlugin.EnableKnifeLinecastBlock.Value
                    && i + 1 < codes.Count
                    && codes[i + 1].opcode == OpCodes.Call
                    && codes[i + 1].operand is MethodInfo mi
                    && mi.Name == "Linecast"
                    && mi.IsStatic)
                {
                    if (ci.opcode == OpCodes.Ldc_I4_1)
                    {
                        ci.opcode = OpCodes.Ldc_I4_0;
                        ci.operand = null;
                    }
                    else if (ci.opcode == OpCodes.Ldc_I4_S && ci.operand is sbyte sb && sb == QueryTriggerIgnore_SByte)
                    {
                        ci.operand = (sbyte)QueryTriggerUseGlobal;
                    }
                    else if (ci.opcode == OpCodes.Ldc_I4 && ci.operand is int iv && iv == QueryTriggerIgnore)
                    {
                        ci.operand = QueryTriggerUseGlobal;
                    }
                }
            }
            return codes;
        }
    }
}
