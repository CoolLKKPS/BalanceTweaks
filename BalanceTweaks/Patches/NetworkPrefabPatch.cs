using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace BalanceTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.SetSingleton))]
    internal static class NetworkPrefabPatch
    {
        private static void Postfix()
        {
            if (!BalanceTweaksPlugin.CreateNetworkPrefab.Value)
                return;
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.PrefabHandler == null)
                return;
            var prefab = new GameObject(BalanceTweaksPlugin.PLUGIN_NAME + " Prefab");
            prefab.hideFlags |= HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(prefab);
            var networkObject = prefab.AddComponent<NetworkObject>();
            try
            {
                var field = typeof(NetworkObject).GetField("GlobalObjectIdHash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    uint hash = (uint)BalanceTweaksPlugin.PLUGIN_GUID.GetHashCode();
                    field.SetValue(networkObject, hash);
                }
            }
            catch
            {
            }
            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(prefab);
        }
    }
}
