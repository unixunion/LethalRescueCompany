using BepInEx.Logging;
using LethalRescueCompanyMod.NetworkBehaviors;
using System;
using Unity.Netcode;

namespace LethalRescueCompanyMod.Models
{
    /**
     * GrabbableObject is abstract, so need this to solve that. 
     **/
    public class LRCGrabbableObject : GrabbableObject
    {

        internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.NetworkBehaviors.LRCGrabbableObject");


        public override void InteractItem()
        {
            log.LogInfo("interact item");
        }


        public override void OnNetworkSpawn()
        {
            log.LogInfo("network spawn");

            RescueCompanyController.ServerEvent += ServerEventHandler;
            RescueCompanyController.ClientEvent += ClientEventHandler;

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {

            RescueCompanyController.ServerEvent -= ServerEventHandler;
            RescueCompanyController.ClientEvent -= ClientEventHandler;

            base.OnNetworkDespawn();
        }

        private void ClientEventHandler(Event @event)
        {
            log.LogInfo($"test: clientevent: {@event}");
        }

        private void ServerEventHandler(Event @event)
        {
            log.LogInfo($"test: serverevent: {@event}");
        }
    }
}
