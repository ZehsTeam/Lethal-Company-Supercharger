using BepInEx.Configuration;
using com.github.zehsteam.Supercharger.MonoBehaviours;
using System.Collections.Generic;
using System.Reflection;

namespace com.github.zehsteam.Supercharger;

public class SyncedConfigManager
{
    private SyncedConfigData _hostConfigData;

    // General Settings
    private ConfigEntry<bool> ExtendedLoggingCfg;
    private ConfigEntry<bool> EnableSuperchargerInOrbitCfg;
    private ConfigEntry<float> SuperchargeChanceCfg;
    private ConfigEntry<int> ItemChargeAmountCfg;
    private ConfigEntry<bool> FlickerShipLightsCfg;
    private ConfigEntry<float> ExplodeChanceCfg;
    private ConfigEntry<int> ExplodeDamageCfg;
    private ConfigEntry<bool> ExplosionTurnsOffShipLightsCfg;

    public bool ExtendedLogging { get { return ExtendedLoggingCfg.Value; } set { ExtendedLoggingCfg.Value = value; } }

    public bool EnableSuperchargerInOrbit
    {
        get
        {
            return _hostConfigData == null ? EnableSuperchargerInOrbitCfg.Value : _hostConfigData.EnableSuperchargerInOrbit;
        }
        set
        {
            EnableSuperchargerInOrbitCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    public float SuperchargeChance
    {
        get
        {
            return _hostConfigData == null ? SuperchargeChanceCfg.Value : _hostConfigData.SuperchargeChance;
        }
        set
        {
            SuperchargeChanceCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    public int ItemChargeAmount
    {
        get
        {
            return _hostConfigData == null ? ItemChargeAmountCfg.Value : _hostConfigData.ItemChargeAmount;
        }
        set
        {
            ItemChargeAmountCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    public bool FlickerShipLights { get { return FlickerShipLightsCfg.Value; } set { FlickerShipLightsCfg.Value = value; } }

    public float ExplodeChance
    {
        get
        {
            return _hostConfigData == null ? ExplodeChanceCfg.Value : _hostConfigData.ExplodeChance;
        }
        set
        {
            ExplodeChanceCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    public int ExplodeDamage
    {
        get
        {
            return _hostConfigData == null ? ExplodeDamageCfg.Value : _hostConfigData.ExplodeDamage;
        }
        set
        {
            ExplodeDamageCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    public bool ExplosionTurnsOffShipLights
    {
        get
        {
            return _hostConfigData == null ? ExplosionTurnsOffShipLightsCfg.Value : _hostConfigData.ExplosionTurnsOffShipLights;
        }
        set
        {
            ExplosionTurnsOffShipLightsCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    public SyncedConfigManager()
    {
        BindConfigs();
        ClearUnusedEntries();
    }

    private void BindConfigs()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        // General Settings
        ExtendedLoggingCfg =             configFile.Bind("General Settings", "ExtendedLogging",             defaultValue: false, "Enable extended logging.");
        EnableSuperchargerInOrbitCfg =   configFile.Bind("General Settings", "EnableSuperchargerInOrbit",   defaultValue: false, "If enabled, the ship's charging station will be able to supercharge while in orbit.");
        SuperchargeChanceCfg =           configFile.Bind("General Settings", "SuperchargeChance",           defaultValue: 45f, new ConfigDescription("The percent chance the ship's charging station will supercharge your item.", new AcceptableValueRange<float>(0.0f, 100.0f)));
        ItemChargeAmountCfg =            configFile.Bind("General Settings", "ItemChargeAmount",            defaultValue: 200, new ConfigDescription("The amount of charge an item will receive after being supercharged.", new AcceptableValueRange<int>(100, 1000)));
        FlickerShipLightsCfg =           configFile.Bind("General Settings", "FlickerShipLights",           defaultValue: true, "If enabled, the ship's lights will flicker during a supercharge.");
        ExplodeChanceCfg =               configFile.Bind("General Settings", "ExplodeChance",               defaultValue: 25f, new ConfigDescription("The percent chance your item will explode from supercharging.", new AcceptableValueRange<float>(0.0f, 100.0f)));
        ExplodeDamageCfg =               configFile.Bind("General Settings", "ExplodeDamage",               defaultValue: 40, new ConfigDescription("The amount of damage you will receive from an explosion.", new AcceptableValueRange<int>(0, 1000)));
        ExplosionTurnsOffShipLightsCfg = configFile.Bind("General Settings", "ExplosionTurnsOffShipLights", defaultValue: true, "If enabled, the ship's lights will turn off after an explosion.");

        EnableSuperchargerInOrbitCfg.SettingChanged += SyncedConfigsChanged;
        SuperchargeChanceCfg.SettingChanged += SyncedConfigsChanged;
        ItemChargeAmountCfg.SettingChanged += SyncedConfigsChanged;
        ExplodeChanceCfg.SettingChanged += SyncedConfigsChanged;
        ExplodeDamageCfg.SettingChanged += SyncedConfigsChanged;
        ExplosionTurnsOffShipLightsCfg.SettingChanged += SyncedConfigsChanged;
    }

    private void ClearUnusedEntries()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
        PropertyInfo orphanedEntriesProp = configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
        orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
        configFile.Save(); // Save the config file to save these changes
    }

    internal void SetHostConfigData(SyncedConfigData syncedConfigData)
    {
        _hostConfigData = syncedConfigData;
    }

    private void SyncedConfigsChanged()
    {
        if (!Plugin.IsHostOrServer) return;

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(this));
    }

    private void SyncedConfigsChanged(object sender, System.EventArgs e)
    {
        SyncedConfigsChanged();
    }
}
