using HarmonyLib;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    internal static class ShotgunConductivePatch
    {
        [HarmonyPostfix]
        private static void MakeShotgunConductive()
        {
            if (!BalanceTweaksPlugin.ShotgunConductive.Value)
                return;

            if (!StartOfRound.Instance.IsHost)
                return;

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
