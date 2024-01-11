using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace LethalRescueCompanyMod.Hacks
{
    public class PowerCheat : MonoBehaviour
    {
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PowerCheat");
        public void Start()
        {
            StartCoroutine(powerUpBaby());
        }

        private IEnumerator powerUpBaby()
        {
            log.LogInfo("Powering up");
            RoundManager.Instance.SwitchPower(true);
            yield return new WaitForSeconds(10);
            Destroy(this);
        }
       
    }
}
