using com.github.zehsteam.Supercharger.MonoBehaviours;
using System.IO;
using UnityEngine;

namespace com.github.zehsteam.Supercharger;

internal class Content
{
    // Prefabs
    public static GameObject NetworkHandlerPrefab { get; private set; }
    public static GameObject SuperchargeStationPrefab { get; private set; }

    public static void Load()
    {
        LoadAssetsFromAssetBundle();
    }

    private static void LoadAssetsFromAssetBundle()
    {
        AssetBundle assetBundle = LoadAssetBundle("supercharger_assets");
        if (assetBundle == null) return;

        // Prefabs
        NetworkHandlerPrefab = LoadAssetFromAssetBundle<GameObject>("NetworkHandler", assetBundle);
        NetworkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();
        SuperchargeStationPrefab = LoadAssetFromAssetBundle<GameObject>("SuperchargeStation", assetBundle);

        Plugin.Logger.LogInfo("Successfully loaded assets from AssetBundle!");
    }

    private static AssetBundle LoadAssetBundle(string fileName)
    {
        try
        {
            string dllFolderPath = Path.GetDirectoryName(Plugin.Instance.Info.Location);
            string assetBundleFilePath = Path.Combine(dllFolderPath, fileName);
            return AssetBundle.LoadFromFile(assetBundleFilePath);
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Failed to load AssetBundle \"{fileName}\". {e}");
        }

        return null;
    }

    private static T LoadAssetFromAssetBundle<T>(string name, AssetBundle assetBundle) where T : Object
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Plugin.Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" from AssetBundle. Name is null or whitespace.");
            return null;
        }

        if (assetBundle == null)
        {
            Plugin.Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. AssetBundle is null.");
            return null;
        }

        T asset = assetBundle.LoadAsset<T>(name);

        if (asset == null)
        {
            Plugin.Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. No asset found with that type and name.");
            return null;
        }

        return asset;
    }
}
