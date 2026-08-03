using HarmonyLib;

namespace BalanceTweaksPlugin
{
    [HarmonyPatch(typeof(BeltBagItem), "PutObjectInBagLocalClient")]
    internal static class BeltBagShotgunAmmoPatch
    {
        [HarmonyPrefix]
        static bool Prefix(GrabbableObject gObject)
        {
            if (gObject is GunAmmo)
            {
                return false;
            }
            return true;
        }
    }
}
