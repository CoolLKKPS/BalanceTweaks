using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(Shovel), "HitShovel")]
    internal static class ShovelTriggerPatch
    {
        static readonly MethodInfo IsTriggerGetter = AccessTools.PropertyGetter(typeof(Collider), "isTrigger");

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!BalanceTweaksPlugin.EnableShovelTriggerHit.Value)
                return instructions;

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt
                    && codes[i].operand is MethodInfo m
                    && m == IsTriggerGetter)
                {
                    codes[i].opcode = OpCodes.Pop;
                    codes[i].operand = null;

                    for (int j = i + 1; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Brtrue || codes[j].opcode == OpCodes.Brtrue_S)
                        {
                            codes[j].opcode = OpCodes.Nop;
                            codes[j].operand = null;
                            break;
                        }
                    }
                    break;
                }
            }
            return codes;
        }
    }
}
