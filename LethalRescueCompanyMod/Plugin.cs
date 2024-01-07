using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.Patches;
using LethalRescueCompanyPlugin.Patches;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
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

            // trying stuff
            GameObject rescueCompany = new GameObject("RescueCompany");
            rescueCompany.AddComponent<RescueCompany>();
            Object.DontDestroyOnLoad((Object)(object)rescueCompany);
            log.LogInfo("Rescue Company Initialized!");


        }

        


    }
}