using HarmonyLib;

namespace BalanceTweaksPlugin
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    internal static class ShotgunWeightPatch
    {
        [HarmonyPostfix]
        static void ModifyShotgunWeight()
        {
            if (!BalanceTweaksPlugin.ShotgunWeight.Value)
                return;

            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.itemName == "Shotgun")
                {
                    item.weight = 1.20f;
                    break;
                }
            }
        }
    }
}
