using System.Collections;
using BepInEx.Logging;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace LethalRescueCompanyMod.Hacks;

public class PowerCheat : MonoBehaviour
{
    internal static ManualLogSource log = Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PowerCheat");

    public void Start()
    {
        StartCoroutine(powerUpBaby());
        if (Settings.TeleportEnabled) StartCoroutine(scotty());
    }

    private IEnumerator powerUpBaby()
    {
        log.LogInfo("Powering up");
        RoundManager.Instance.SwitchPower(true);

        yield return new WaitForSeconds(10);
        //Destroy(this);
    }

    private IEnumerator scotty()
    {
        log.LogInfo("Beam me up!");
        yield return new WaitForSeconds(3);
        var e = RoundManager.FindMainEntranceScript();

        if (e != null)
            e.TeleportPlayer();
        else
            log.LogWarning("No MainEnterance to hack");


        Destroy(this);
    }
}