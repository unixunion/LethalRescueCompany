using System.Collections;
using BepInEx.Logging;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace LethalRescueCompanyMod;

public class WelcomeMessage : MonoBehaviour
{
    internal static ManualLogSource log = Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.WelcomeMessage");
    public bool hasShownWelcome;

    public WelcomeMessage()
    {
        if (!hasShownWelcome) StartCoroutine(showWelcome());
    }


    private IEnumerator showWelcome()
    {
        yield return new WaitForSeconds(10);
        HUDManager.Instance.DisplayTip("Lethal Rescue Company",
            "You can rescue your cacooned crew by bringing their bodies back to the ship");
    }
}