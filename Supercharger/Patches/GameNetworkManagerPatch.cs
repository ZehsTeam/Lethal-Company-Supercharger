using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal static class GameNetworkManagerPatch
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        AddNetworkPrefabs();
    }

    private static void AddNetworkPrefabs()
    {
        AddNetworkPrefab(Content.NetworkHandlerPrefab);
    }

    private static void AddNetworkPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Plugin.Logger.LogError("Failed to register network prefab. GameObject is null.");
            return;
        }

        NetworkManager.Singleton.AddNetworkPrefab(prefab);

        Plugin.Logger.LogInfo($"Registered \"{prefab.name}\" network prefab.");
    }
}
