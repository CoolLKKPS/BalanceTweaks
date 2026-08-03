using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        const float FloatComparisonEpsilon = 0.0001f;

        const int QueryTriggerIgnore = 1;
        const int QueryTriggerUseGlobal = 0;

        const sbyte QueryTriggerIgnore_SByte = 1;

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction ci = codes[i];

                if (ci.opcode == OpCodes.Ldc_R4 && ci.operand is float f)
                {
                    if (Math.Abs(f - OriginalSphereCastRadius) < FloatComparisonEpsilon)
                        ci.operand = SphereCastRadius;
                    else if (Math.Abs(f - OriginalSphereCastDistance) < FloatComparisonEpsilon)
                        ci.operand = SphereCastDistance;
                }

                if (BalanceTweaksPlugin.EnableShovelLinecastBlock.Value
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
