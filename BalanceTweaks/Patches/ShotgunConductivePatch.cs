using HarmonyLib;

namespace BalanceTweaksPlugin
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    internal static class ShotgunConductivePatch
    {
        [HarmonyPostfix]
        static void MakeShotgunConductive()
        {
            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.itemName == "Shotgun")
                {
                    item.isConductiveMetal = true;
                    break;
                }
            }
        }
    }
}
