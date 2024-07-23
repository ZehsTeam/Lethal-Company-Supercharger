using System;
using Unity.Netcode;

namespace com.github.zehsteam.Supercharger;

[Serializable]
public class SyncedConfigData : INetworkSerializable
{
    // General Settings
    public bool EnableSuperchargerInOrbit;
    public float SuperchargeChance;
    public int ItemChargeAmount;
    public float ExplodeChance;
    public int ExplodeDamage;
    public bool ExplosionTurnsOffShipLights;

    public SyncedConfigData() { }

    public SyncedConfigData(SyncedConfigManager configManager)
    {
        // General Settings
        EnableSuperchargerInOrbit = configManager.EnableSuperchargerInOrbit;
        SuperchargeChance = configManager.SuperchargeChance;
        ItemChargeAmount = configManager.ItemChargeAmount;
        ExplodeChance = configManager.ExplodeChance;
        ExplodeDamage = configManager.ExplodeDamage;
        ExplosionTurnsOffShipLights = configManager.ExplosionTurnsOffShipLights;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // General Settings
        serializer.SerializeValue(ref EnableSuperchargerInOrbit);
        serializer.SerializeValue(ref SuperchargeChance);
        serializer.SerializeValue(ref ItemChargeAmount);
        serializer.SerializeValue(ref ExplodeChance);
        serializer.SerializeValue(ref ExplodeDamage);
        serializer.SerializeValue(ref ExplosionTurnsOffShipLights);
    }
}
