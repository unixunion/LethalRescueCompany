using BepInEx.Logging;
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
            StartCoroutine(scotty());
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
            EntranceTeleport e = RoundManager.FindMainEntranceScript();

            if (e != null)
            {
                e.TeleportPlayer();
            }
            else
            {
                log.LogWarning("No MainEnterance to hack");
            }

            
            Destroy(this);
        }


        
     
       
    }
}
