using HarmonyLib;

[HarmonyPatch(typeof(StartOfRound), "Start")]
internal static class ShotgunWeightPatch
{
    [HarmonyPostfix]
    static void ModifyShotgunWeight()
    {
        foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
        {
            if (item.itemName == "Shotgun")
            {
                item.weight = 1.5f;
                break;
            }
        }
    }
}
