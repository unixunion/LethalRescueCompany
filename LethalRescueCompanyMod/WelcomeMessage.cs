using BepInEx.Logging;
using System.Collections;
using UnityEngine;

namespace LethalRescueCompanyMod
{
    public class WelcomeMessage : MonoBehaviour
    {
        public bool hasShownWelcome = false;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.WelcomeMessage");
        public WelcomeMessage()
        {
            if (!hasShownWelcome)
            {
                StartCoroutine(showWelcome());
            }    

        }


        private IEnumerator showWelcome()
        {
            yield return new WaitForSeconds(10);
            HUDManager.Instance.DisplayTip("Lethal Rescue Company", "You can rescue your cacooned crew by bringing their bodies back to the ship");
        }
    }
}
