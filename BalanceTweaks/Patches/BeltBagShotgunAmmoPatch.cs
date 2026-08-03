using HarmonyLib;
using Unity.Netcode;

namespace BalanceTweaksPlugin
{
    [HarmonyPatch(typeof(BeltBagItem), "TryAddObjectToBagServerRpc")]
    internal static class BeltBagShotgunAmmoPatch
    {
        [HarmonyPrefix]
        static bool Prefix(BeltBagItem __instance, NetworkObjectReference netObjectRef, int playerWhoAdded)
        {
            if (!__instance.IsHost)
                return true;

            if (!BalanceTweaksPlugin.BeltBagShotgunAmmo.Value)
                return true;

            NetworkObject networkObject;
            if (netObjectRef.TryGet(out networkObject, null))
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
