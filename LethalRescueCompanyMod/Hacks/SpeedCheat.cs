using BepInEx.Logging;
using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace LethalRescueCompanyMod.Hacks
{
    internal class SpeedCheat : MonoBehaviour
    {
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PowerCheat");
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
}
