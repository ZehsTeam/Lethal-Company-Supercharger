using BepInEx;
using BepInEx.Logging;
using com.github.zehsteam.Supercharger.Patches;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace com.github.zehsteam.Supercharger;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance;
    internal static new ManualLogSource Logger;

    internal static ConfigManager ConfigManager;

    #pragma warning disable IDE0051 // Remove unused private members
    private void Awake()
    #pragma warning restore IDE0051 // Remove unused private members
    {
        if (Instance == null) Instance = this;

        Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        _harmony.PatchAll(typeof(GameNetworkManagerPatch));
        _harmony.PatchAll(typeof(StartOfRoundPatch));
        _harmony.PatchAll(typeof(InteractTriggerPatch));
        _harmony.PatchAll(typeof(ItemChargerPatch));

        ConfigManager = new ConfigManager();

        Content.Load();

        NetcodePatcherAwake();
    }

    private void NetcodePatcherAwake()
    {
        try
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var types = currentAssembly.GetTypes();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    try
                    {
                        // Safely attempt to retrieve custom attributes
                        var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                        if (attributes.Length > 0)
                        {
                            try
                            {
                                // Safely attempt to invoke the method
                                method.Invoke(null, null);
                            }
                            catch (TargetInvocationException ex)
                            {
                                // Log and continue if method invocation fails (e.g., due to missing dependencies)
                                Logger.LogWarning($"Failed to invoke method {method.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        // Handle errors when fetching custom attributes, due to missing types or dependencies
                        Logger.LogWarning($"Error processing method {method.Name} in type {type.Name}: {ex.Message}");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            // Catch any general exceptions that occur in the process
            Logger.LogError($"An error occurred in NetcodePatcherAwake: {ex.Message}");
        }
    }

    public void LogInfoExtended(object data)
    {
        LogExtended(LogLevel.Info, data);
    }

    public void LogExtended(LogLevel level, object data)
    {
        if (ConfigManager == null || ConfigManager.ExtendedLogging == null)
        {
            Logger.Log(level, data);
            return;
        }

        if (ConfigManager.ExtendedLogging.Value)
        {
            Logger.Log(level, data);
        }
    }
}
