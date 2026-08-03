using HarmonyLib;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    internal static class KnifeWeightPatch
    {
        [HarmonyPostfix]
        static void ModifyKnifeWeight()
        {
            if (!BalanceTweaksPlugin.KnifeWeight.Value)
                return;

            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.itemName == "Kitchen knife")
                {
                    item.weight = 1.05f;
                    break;
                }
            }
        }
    }
}
