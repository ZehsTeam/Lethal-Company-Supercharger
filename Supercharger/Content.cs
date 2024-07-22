using com.github.zehsteam.Supercharger.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.Supercharger;

internal class Content
{
    // Network Handler
    public static GameObject NetworkHandlerPrefab;

    // SuperchargeStation
    public static GameObject SuperchargeStationPrefab;

    public static void Load()
    {
        LoadAssetsFromAssetBundle();
    }

    private static void LoadAssetsFromAssetBundle()
    {
        try
        {
            var dllFolderPath = System.IO.Path.GetDirectoryName(Plugin.Instance.Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "supercharger_assets");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            // Network Handler
            NetworkHandlerPrefab = assetBundle.LoadAsset<GameObject>("NetworkHandler");
            NetworkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();

            // SuperchargeStation
            SuperchargeStationPrefab = assetBundle.LoadAsset<GameObject>("SuperchargeStation");

            Plugin.logger.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (System.Exception e)
        {
            Plugin.logger.LogError($"Error: failed to load assets from AssetBundle.\n\n{e}");
        }
    }
}
