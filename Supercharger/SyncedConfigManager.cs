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
    private ConfigEntry<float> SuperchargeChanceCfg;
    private ConfigEntry<float> ExplodeChanceCfg;

    public bool ExtendedLogging { get { return ExtendedLoggingCfg.Value; } set { ExtendedLoggingCfg.Value = value; } }

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

    public SyncedConfigManager()
    {
        BindConfigs();
        ClearUnusedEntries();
    }

    private void BindConfigs()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        // General Settings
        ExtendedLoggingCfg = configFile.Bind("General Settings", "ExtendedLogging", defaultValue: false, "Enable extended logging.");
        SuperchargeChanceCfg = configFile.Bind("General Settings", "SuperchargeChance", defaultValue: 50f, new ConfigDescription("The percent chance a charge station will supercharge your item.", new AcceptableValueRange<float>(0.0f, 100.0f)));
        ExplodeChanceCfg = configFile.Bind("General Settings", "ExplodeChance", defaultValue: 50f, new ConfigDescription("The percent chance your item will explode from supercharging.", new AcceptableValueRange<float>(0.0f, 100.0f)));
        
        SuperchargeChanceCfg.SettingChanged += SyncedConfigsChanged;
        ExplodeChanceCfg.SettingChanged += SyncedConfigsChanged;
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
        if (!Plugin.IsHostOrServer) return;

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(this));
    }
}
