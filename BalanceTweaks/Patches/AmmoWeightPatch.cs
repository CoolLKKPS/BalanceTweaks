using HarmonyLib;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    internal static class AmmoWeightPatch
    {
        [HarmonyPostfix]
        static void ModifyAmmoWeight()
        {
            if (!BalanceTweaksPlugin.AmmoWeight.Value)
                return;

            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.itemName == "Ammo")
                {
                    item.weight = 1f;
                    break;
                }
            }
        }
    }
}
