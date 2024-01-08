using BepInEx.Logging;
using Dissonance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            HUDManager.Instance.DisplayTip("Lethal Rescue Company", "You can rescue your crewmates that have been webbed up by spiders by bringing them back to the ship");
        }
    }
}
