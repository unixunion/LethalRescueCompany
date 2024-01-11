using BepInEx.Logging;

namespace LethalRescueCompanyMod.Models
{
    /**
     * GrabbableObject is abstract, so need this to solve that. 
     **/
    public class LRCGrabbableObject : GrabbableObject
    {

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.NetworkBehaviors.LRCGrabbableObject");

        public override void InteractItem()
        {
            log.LogInfo("interact item");
        }
    }
}
