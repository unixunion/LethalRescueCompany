using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.NetworkBehaviors;
using LethalRescueCompanyMod.Patches;
using LethalRescueCompanyPlugin.Patches;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Unity.Netcode;
using UnityEngine;


namespace LethalRescueCompanyPlugin
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalCompanyMemorableMomentsPlugin : BaseUnityPlugin
    {
        private const string modGUID = "com.lethalcompany.rescuecompany";
        private const string modName = "Lethal Rescue Company";
        private const string modVersion = "1.0.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        public static LethalCompanyMemorableMomentsPlugin instance;
        public static GameObject prefab {  get; private set; }

        internal ManualLogSource log;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            log = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            log.LogInfo("Rescue Company Initializing...");

            loadPrefab();

            harmony.PatchAll(typeof(LethalCompanyMemorableMomentsPlugin));
            harmony.PatchAll(typeof(SandSpiderAIPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(HudManagerPatch));
            //harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));

        }

        private void loadPrefab()
        {
            if (prefab == null)
            {
                log.LogInfo("attempting to load prefab");
                var MainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "prefabs"));
                var networkPrefab = (GameObject)MainAssetBundle.LoadAsset("assets/lethalrescuenetworkprefab.prefab");
                prefab = networkPrefab;
                log.LogInfo($"prefab set to {prefab}");
            } 
        }

        public GameObject getPrefab()
        {
            loadPrefab();
            if (prefab != null)
            {
                return prefab;
            }
            else
            {
                log.LogError("failed to load the prefab");
            }

            return null;

        }
    }
}