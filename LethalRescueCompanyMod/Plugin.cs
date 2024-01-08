using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.NetworkBehaviors;
using LethalRescueCompanyMod.Patches;
using LethalRescueCompanyPlugin.Patches;
using System.Diagnostics;
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
        private static LethalCompanyMemorableMomentsPlugin instance;
        internal ManualLogSource log;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            log = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            log.LogInfo("Rescue Company Initializing...");

            harmony.PatchAll(typeof(LethalCompanyMemorableMomentsPlugin));
            harmony.PatchAll(typeof(SandSpiderAIPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(HudManagerPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));

            log.LogInfo("Initializing the ping pooog");
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = false;
            GameObject reviveStoreGameObject = new GameObject("RescueCompanyPingPong");
            reviveStoreGameObject.AddComponent<RescueCompanyPingPong>();
            DontDestroyOnLoad(reviveStoreGameObject);

            NetworkManager.Singleton.AddNetworkPrefab(reviveStoreGameObject.gameObject);

        }
    }
}