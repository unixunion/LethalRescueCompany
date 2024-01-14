using UnityEngine;

namespace LethalRescueCompanyMod;

/**
 * The accessor for the settings, maybe move this to pluginConfig...
 */
public static class Settings
{
    public static bool IsDebug { get; set; } = false;
    public static bool IsSolo { get; set; } = false;

    public static bool TeleportEnabled { get; set; } = false;
    public static bool DebugAddRevive { get; set; } = false;
    public static GameObject HangingBodyPrefab { get; set; } = null;
}