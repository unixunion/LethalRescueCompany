using System.Collections;
using BepInEx.Logging;
using GameNetcodeStuff;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace LethalRescueCompanyMod.Hacks;

internal class SpeedCheat : MonoBehaviour
{
    internal static ManualLogSource log = Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PowerCheat");

    public void Start()
    {
        StartCoroutine(Meth());
    }

    private IEnumerator Meth()
    {
        log.LogInfo("Snorting the cocain");
        yield return new WaitForSeconds(5);
        gameObject.GetComponentInParent<PlayerControllerB>().movementSpeed = 10f;
        //Destroy(this);
    }
}