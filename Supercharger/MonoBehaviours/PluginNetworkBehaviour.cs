using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.MonoBehaviours;

public class PluginNetworkBehaviour : NetworkBehaviour
{
    public static PluginNetworkBehaviour Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [ClientRpc]
    public void SendConfigToPlayerClientRpc(SyncedConfigData syncedConfigData, ClientRpcParams clientRpcParams = default)
    {
        if (Plugin.IsHostOrServer) return;

        Plugin.logger.LogInfo("Syncing config with host.");

        Plugin.ConfigManager.SetHostConfigData(syncedConfigData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SuperchargeItemServerRpc(int fromPlayerId)
    {
        SuperchargeItemClientRpc(fromPlayerId);
    }

    [ClientRpc]
    public void SuperchargeItemClientRpc(int fromPlayerId)
    {
        PlayerControllerB playerScript = PlayerUtils.GetPlayerScript(fromPlayerId);
        if (PlayerUtils.IsLocalPlayer(playerScript)) return;

        if (ShipHelper.TryGetSuperchargeStationBehaviour(out SuperchargeStationBehaviour superchargeStationBehaviour))
        {
            superchargeStationBehaviour.SuperchargeItemForOtherClient(playerScript);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnExplosionServerRpc(Vector3 position, int damage, float range)
    {
        SpawnExplosionClientRpc(position, damage, range);
    }

    [ClientRpc]
    public void SpawnExplosionClientRpc(Vector3 position, int damage, float range)
    {
        Landmine.SpawnExplosion(position, spawnExplosionEffect: true, killRange: 0f, damageRange: range, nonLethalDamage: damage);
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
