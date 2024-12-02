using BepInEx.Configuration;
using com.github.zehsteam.Supercharger.Data;
using com.github.zehsteam.Supercharger.Helpers;

namespace com.github.zehsteam.Supercharger;

public class ConfigManager
{
    // General
    public ConfigEntry<bool> ExtendedLogging { get; private set; }

    // Supercharger
    public SyncedConfigEntry<float> Supercharger_Chance { get; private set; }
    public SyncedConfigEntry<int> Supercharger_ItemChargeAmount { get; private set; }
    public ConfigEntry<bool> Supercharger_FlickerShipLights { get; private set; }
    public SyncedConfigEntry<bool> Supercharger_OnlyWhenFullyCharged { get; private set; }
    public SyncedConfigEntry<bool> Supercharger_EnableInOrbit { get; private set; }
    
    // Explosion
    public SyncedConfigEntry<float> Explosion_Chance { get; private set; }
    public SyncedConfigEntry<float> Explosion_Range { get; private set; }
    public SyncedConfigEntry<int> Explosion_Damage { get; private set; }
    public SyncedConfigEntry<int> Explosion_EnemyDamage { get; private set; }
    public SyncedConfigEntry<bool> Explosion_TurnsOffShipLights { get; private set; }

    public ConfigManager()
    {
        BindConfigs();
        ConfigHelper.ClearUnusedEntries();
    }

    private void BindConfigs()
    {
        ConfigHelper.SkipAutoGen();

        // General
        ExtendedLogging = ConfigHelper.Bind("General", "ExtendedLogging", defaultValue: false, requiresRestart: false, "Enable extended logging.");

        // Supercharger
        Supercharger_Chance =               new SyncedConfigEntry<float>("Supercharger", "Chance",               defaultValue: 45f,   "The percent chance the ship's charging station will supercharge your item.", new AcceptableValueRange<float>(0.0f, 100.0f));
        Supercharger_ItemChargeAmount =     new SyncedConfigEntry<int>(  "Supercharger", "ItemChargeAmount",     defaultValue: 200,   "The amount of charge an item will receive after being supercharged.", new AcceptableValueRange<int>(100, 1000));
        Supercharger_FlickerShipLights =    ConfigHelper.Bind(           "Supercharger", "FlickerShipLights",    defaultValue: true,  requiresRestart: false, "If enabled, the ship's lights will flicker during a supercharge.");
        Supercharger_OnlyWhenFullyCharged = new SyncedConfigEntry<bool>( "Supercharger", "OnlyWhenFullyCharged", defaultValue: false, "If enabled, the ship's charging station will only be able to supercharge when the item has a battery charge of 100 or more.");
        Supercharger_EnableInOrbit =        new SyncedConfigEntry<bool>( "Supercharger", "EnableInOrbit",        defaultValue: false, "If enabled, the ship's charging station will be able to supercharge while in orbit.");

        // Explosion
        Explosion_Chance =             new SyncedConfigEntry<float>("Explosion", "Chance",             defaultValue: 30f,  "The percent chance your item will explode from supercharging.", new AcceptableValueRange<float>(0.0f, 100.0f));
        Explosion_Range =              new SyncedConfigEntry<float>("Explosion", "Range",              defaultValue: 4,    "The range of the explosion in meters.", new AcceptableValueRange<float>(0.0f, 8.0f));
        Explosion_Damage =             new SyncedConfigEntry<int>(  "Explosion", "Damage",             defaultValue: 40,   "The amount of damage you will receive from an explosion.", new AcceptableValueRange<int>(0, 1000));
        Explosion_EnemyDamage =        new SyncedConfigEntry<int>(  "Explosion", "EnemyDamage",        defaultValue: 2,    "The amount of damage enemies will receive from an explosion.", new AcceptableValueRange<int>(0, 100));
        Explosion_TurnsOffShipLights = new SyncedConfigEntry<bool>( "Explosion", "TurnsOffShipLights", defaultValue: true, "If enabled, the ship's lights will turn off after an explosion.");
    }
}
