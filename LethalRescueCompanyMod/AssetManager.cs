using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace LethalRescueCompanyMod;

public static class AssetManager
{
    private static readonly ManualLogSource log = Logger.CreateLogSource("LethalRescueCompanyPlugin.AssetManager");
    public static Dictionary<string, GameObject> assetMappings { get; private set; }
    private static bool initialized { get; set; }

    /**
     * Loads all assets from the "prefabs" file, uses their object name as the key as defined in unity editor.
     */
    internal static void LoadAssets()
    {
        log.LogInfo("attempting to load prefabs");
        assetMappings = new Dictionary<string, GameObject>();

        //log.LogInfo("printing out ALL found assets");

        //var OriginalAssets = Resources.LoadAll("./prefabs");
        //foreach (var item in OriginalAssets)
        //{
        //    log.LogInfo($"asset: {item.name}");
        //}


        var MainAssetBundle =
            AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "prefabs"));
        foreach (var item in MainAssetBundle.GetAllAssetNames())
        {
            var prefab = (GameObject)MainAssetBundle.LoadAsset(item);
            log.LogInfo($"adding asset: {prefab.name}, {item}");
            assetMappings.Add(prefab.name, prefab);
        }

        initialized = true;
    }

    public static GameObject GetAssetByKey(string key)
    {
        if (assetMappings.Count == 0 && !initialized) LoadAssets();

        if (assetMappings.TryGetValue(key, out var asset))
        {
            // Found the asset, return it
            return asset;
        }

        // Asset not found
        log.LogError($"Asset {key} not found, check the name matches what is set in unity editor!");
        return null;
    }
}