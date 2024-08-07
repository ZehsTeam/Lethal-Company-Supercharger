﻿using com.github.zehsteam.Supercharger.MonoBehaviours;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    static void AwakePatch()
    {
        SpawnNetworkHandler();
    }

    private static void SpawnNetworkHandler()
    {
        if (!Plugin.IsHostOrServer) return;

        var networkHandlerHost = Object.Instantiate(Content.NetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
    [HarmonyPrefix]
    static void OnClientConnectPatch(ref ulong clientId)
    {
        SendConfigToNewConnectedPlayer(clientId);
    }

    private static void SendConfigToNewConnectedPlayer(ulong clientId)
    {
        if (!Plugin.IsHostOrServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = [clientId]
            }
        };

        Plugin.logger.LogInfo($"Sending config to client: {clientId}");

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(Plugin.ConfigManager), clientRpcParams);
    }

    [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
    [HarmonyPrefix]
    static void OnLocalDisconnectPatch()
    {
        Plugin.Instance.OnLocalDisconnect();
    }

    [HarmonyPatch(nameof(StartOfRound.PowerSurgeShip))]
    [HarmonyPostfix]
    static void PowerSurgeShipPatch()
    {
        ShipHelper.PowerSurgedShip = true;
    }
}
