using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyMod.Patches;
using LethalRescueCompanyPlugin;
using UnityEngine;

namespace LethalRescueCompanyMod;

[BepInPlugin(modGUID, modName, modVersion)]
public class LethalCompanyMemorableMomentsPlugin : BaseUnityPlugin
{
    private const string modGUID = "com.lethalcompany.rescuecompany";
    private const string modName = "Lethal Rescue Company";
    private const string modVersion = "1.0.0.0";
    public static LethalCompanyMemorableMomentsPlugin instance;
    private readonly Harmony harmony = new(modGUID);


    internal ManualLogSource log;
    public static PluginConfig cfg { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;

        cfg = new PluginConfig(Config);
        cfg.InitBindings();

        Settings.IsDebug = cfg.debug;

        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0) method.Invoke(null, null);
            }
        }

        log = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        log.LogInfo("Rescue Company Initializing...");

        // notify the asset manager to load its shit. 
        AssetManager.LoadAssets();

        harmony.PatchAll(typeof(LethalCompanyMemorableMomentsPlugin));
        harmony.PatchAll(typeof(SandSpiderAIPatch));
        harmony.PatchAll(typeof(PlayerControllerBPatch));
        harmony.PatchAll(typeof(HudManagerPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(GameNetworkManagerPatch));
    }
}