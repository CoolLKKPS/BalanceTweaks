using HarmonyLib;
using Unity.Netcode;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(BeltBagItem), "TryAddObjectToBagServerRpc")]
    internal static class BeltBagShotgunAmmoPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BeltBagItem __instance, NetworkObjectReference netObjectRef, int playerWhoAdded)
        {
            if (!__instance.IsHost)
                return true;

            if (!BalanceTweaksPlugin.BeltBagShotgunAmmo.Value)
                return true;

            if (netObjectRef.TryGet(out NetworkObject networkObject, null))
            {
                if (networkObject.GetComponent<GrabbableObject>() is GunAmmo)
                {
                    __instance.CancelAddObjectToBagClientRpc(playerWhoAdded);
                    return false;
                }
            }
            return true;
        }
    }
}
