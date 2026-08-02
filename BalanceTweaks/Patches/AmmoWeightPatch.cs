using HarmonyLib;

[HarmonyPatch(typeof(StartOfRound), "Start")]
internal static class AmmoWeightPatch
{
    [HarmonyPostfix]
    static void ModifyAmmoWeight()
    {
        foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
        {
            if (item.itemName == "Ammo")
            {
                item.weight = 1.13f;
                break;
            }
        }
    }
}
