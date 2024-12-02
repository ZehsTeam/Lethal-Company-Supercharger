using com.github.zehsteam.Supercharger.Data;
using com.github.zehsteam.Supercharger.Helpers;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    private static void AwakePatch()
    {
        SpawnNetworkHandler();
    }

    private static void SpawnNetworkHandler()
    {
        if (!NetworkUtils.IsServer) return;

        var networkHandlerHost = Object.Instantiate(Content.NetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
    [HarmonyPrefix]
    private static void OnClientConnectPatch(ref ulong clientId)
    {
        SendConfigToNewConnectedPlayer(clientId);
    }

    private static void SendConfigToNewConnectedPlayer(ulong clientId)
    {
        if (!NetworkUtils.IsServer) return;

        SyncedConfigEntryBase.SendConfigsToClient(clientId);
    }

    [HarmonyPatch(nameof(StartOfRound.PowerSurgeShip))]
    [HarmonyPostfix]
    private static void PowerSurgeShipPatch()
    {
        ShipHelper.PowerSurgedShip = true;
    }
}
