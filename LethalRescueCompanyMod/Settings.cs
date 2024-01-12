using UnityEngine;

namespace LethalRescueCompanyMod
{
    /**
     * The accessor for the settings, maybe move this to pluginConfig...
     **/
    public static class Settings
    {
        public static bool isDebug { get; set; } = true;
        public static bool isSolo { get; set; } = false;
        public static bool debugAddRevive { get; set; } = false;
        public static GameObject hangingBodyPrefab { get; set; } = null;
    }
}
