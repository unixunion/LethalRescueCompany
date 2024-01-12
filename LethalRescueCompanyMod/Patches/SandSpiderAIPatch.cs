using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyMod.Models;

namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SandSpiderAIPatch : BaseUnityPlugin
    {

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.SandSpiderAIPatch");
        static Helper helper = new Helper();
        static DeadBodyInfo currentlyHeldBody = null;
        

        [HarmonyPatch("HangBodyFromCeiling")]
        [HarmonyPrefix]
        static void hangBodyFromCeilingPrePatch(ref DeadBodyInfo ___currentlyHeldBody)
        {
            log.LogInfo($"Getting currentlyHeldBody: {___currentlyHeldBody}");
            currentlyHeldBody = ___currentlyHeldBody;
            for (int i = 0; i < currentlyHeldBody.bodyBloodDecals.Length; i++)
            {
                currentlyHeldBody.bodyBloodDecals[i].SetActive(value: false);
            }
        }


        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void ExtractFields(ref SandSpiderAI __instance)
        {
            Settings.hangingBodyPrefab = __instance.hangBodyPhysicsPrefab;
        }


        [HarmonyPatch("HangBodyFromCeiling")]
        [HarmonyPostfix]
        static void hangBodyFromCeilingPostPatch(ref SandSpiderAI __instance)
        {
            makeWebbedBodyGrabbable(currentlyHeldBody);

            // if debug / solo
            if (Settings.isDebug && Settings.isSolo)
            {
                log.LogInfo("debug and solo, spider hacks to revive player");
                // spawn the player
                //helper.ReviveRescuedPlayer(currentlyHeldBody, StartOfRound.Instance);
                helper.ReviveRescuedPlayer(currentlyHeldBody.playerScript, currentlyHeldBody.transform.position);

                RoundManager.Instance.currentLevel.Enemies.Clear();
                RoundManager.Instance.currentLevel.OutsideEnemies.Clear();
                RoundManager.Instance.currentLevel.DaytimeEnemies.Clear();

                log.LogInfo("Destroying Self");
                Destroy(__instance.gameObject);
                Destroy(__instance);
            }
        }

        private static void makeWebbedBodyGrabbable(DeadBodyInfo deadBodyInfo)
        {
            if (Settings.isDebug) log.LogInfo($"Making wrapped body grabbable, currently attached to: {deadBodyInfo.attachedTo.name}");

            //deadBodyInfo.secondaryAttachedTo = deadBodyInfo.attachedTo;
            //deadBodyInfo.attachedTo = null;
            //if (Settings.isDebug) log.LogInfo($"reattached via secondary to the primary attachment point");

            deadBodyInfo.canBeGrabbedBackByPlayers = true;
            
            // according to code, this is not needed, as the above line overrides it in the late update.
            //deadBodyInfo.grabBodyObject.grabbable = true;
            

            //var t = deadBodyInfo.GetComponent<RagdollGrabbableObject>();
            //if (t != null) t.grabbable = true;


            if (deadBodyInfo.gameObject != null)
            {
                
                if (deadBodyInfo.gameObject.GetComponent<RevivableTrait>() == null)
                {
                    if (Settings.isDebug) log.LogInfo("adding revivable trait");
                    deadBodyInfo.gameObject.AddComponent<RevivableTrait>();
                } else
                {
                    if (Settings.isDebug) log.LogInfo("trait already present");
                }

                // configure revivable TEMPORARY DISABLE DEBUG
                var revivableTrait = deadBodyInfo.gameObject.GetComponent<RevivableTrait>();
                //var testrd = deadBodyInfo.GetComponentInParent<RagdollGrabbableObject>();
                //log.LogInfo($"parent ragdoll: {testrd}");
                //var ragdollGrabbableObject = deadBodyInfo.gameObject.AddComponent<LRCGrabbableObject>();
                //if (ragdollGrabbableObject == null) log.LogWarning("unable to find grabbable, fix this!");
                revivableTrait.setGrabbable(deadBodyInfo.grabBodyObject);
                revivableTrait.setPlayerControllerB(deadBodyInfo.playerScript);

            }
            else
            {
                log.LogError("deadbody gameobject was null, error making it revivable");
            }

            if (Settings.isDebug)
            {
                log.LogInfo("lets inspect...");
                log.LogInfo($"db....grabbable: {deadBodyInfo.grabBodyObject.grabbable}, " +
                            $"db.canBeGrabbedBackByPlayers: {deadBodyInfo.canBeGrabbedBackByPlayers}, " +
                            $"db....hasHitGround: {deadBodyInfo.grabBodyObject.hasHitGround}, " +
                            $"db<ReviveTrait>: {deadBodyInfo.gameObject.GetComponent<RevivableTrait>()}");
            }

        }

    }
}
