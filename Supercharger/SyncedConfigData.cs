using System;
using Unity.Netcode;

namespace com.github.zehsteam.Supercharger;

[Serializable]
public class SyncedConfigData : INetworkSerializable
{
    // General Settings
    public float SuperchargeChance;
    public float ExplodeChance;

    public SyncedConfigData() { }

    public SyncedConfigData(SyncedConfigManager configManager)
    {
        // General Settings
        SuperchargeChance = configManager.SuperchargeChance;
        ExplodeChance = configManager.ExplodeChance;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // General Settings
        serializer.SerializeValue(ref SuperchargeChance);
        serializer.SerializeValue(ref ExplodeChance);
    }
}
