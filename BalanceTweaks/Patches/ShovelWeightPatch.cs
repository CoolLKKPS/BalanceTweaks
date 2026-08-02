using HarmonyLib;

[HarmonyPatch(typeof(StartOfRound), "Start")]
internal static class ShovelWeightPatch
{
    [HarmonyPostfix]
    static void ModifyShovelWeight()
    {
        foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
        {
            if (item.itemName == "Shovel")
            {
                item.weight = 1.18f;
                break;
            }
        }
    }
}
