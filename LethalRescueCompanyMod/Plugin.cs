using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyPlugin.Patches;

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

            log.LogInfo("I am alive!");
            harmony.PatchAll(typeof(LethalCompanyMemorableMomentsPlugin));
            harmony.PatchAll(typeof(PlayerControllerBPatch));

        }
    }
}