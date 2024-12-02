using com.github.zehsteam.Supercharger.Data;
using com.github.zehsteam.Supercharger.Helpers;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.MonoBehaviours;

internal class PluginNetworkBehaviour : NetworkBehaviour
{
    public static PluginNetworkBehaviour Instance { get; private set; }

    private void Awake()
    {
        // Ensure there is only one instance of the Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate object
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance != null && Instance != this)
        {
            // Ensure only the server can handle despawning duplicate instances
            if (IsServer)
            {
                NetworkObject.Despawn(); // Despawn the networked object
            }

            return;
        }

        Instance = this;
    }

    [ClientRpc]
    public void SetSyncedConfigValueClientRpc(string section, string key, string value, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkUtils.IsServer) return;

        SyncedConfigEntryBase.SetValueFromServer(section, key, value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SuperchargeItemServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var senderClientId = serverRpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.ContainsKey(senderClientId)) return;

        SuperchargeItemClientRpc(senderClientId);
    }

    [ClientRpc]
    public void SuperchargeItemClientRpc(ulong senderClientId)
    {
        if (NetworkUtils.IsLocalClientId(senderClientId)) return;

        PlayerControllerB playerScript = PlayerUtils.GetPlayerScriptByClientId(senderClientId);

        if (ShipHelper.TryGetSuperchargeStationBehaviour(out SuperchargeStationBehaviour superchargeStationBehaviour))
        {
            superchargeStationBehaviour.SuperchargeItemForOtherClient(playerScript);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnExplosionServerRpc(Vector3 position, int damage, int enemyDamage, float range, ulong senderClientId)
    {
        SpawnExplosionClientRpc(position, damage, enemyDamage, range, senderClientId);
    }
    
    [ClientRpc]
    public void SpawnExplosionClientRpc(Vector3 position, int damage, int enemyDamage, float range, ulong senderClientId)
    {
        PlayerControllerB playerScript = PlayerUtils.GetPlayerScriptByClientId(senderClientId);

        Utils.CreateExplosion(position, spawnExplosionEffect: true, damage: damage, maxDamageRange: range, enemyHitForce: enemyDamage, attacker: playerScript);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PowerSurgeShipServerRpc()
    {
        PowerSurgeShipClientRpc();
    }

    [ClientRpc]
    public void PowerSurgeShipClientRpc()
    {
        StartOfRound.Instance.PowerSurgeShip();
    }
}
