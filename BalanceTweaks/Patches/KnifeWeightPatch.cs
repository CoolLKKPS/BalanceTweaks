using HarmonyLib;

namespace BalanceTweaksPlugin
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    internal static class KnifeWeightPatch
    {
        [HarmonyPostfix]
        static void ModifyKnifeWeight()
        {
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
