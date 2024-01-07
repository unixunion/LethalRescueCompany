using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LethalRescueCompanyMod
{
    public class RescueCompany : NetworkBehaviour
    {

        public static RescueCompany instance;

        public void RevivePlayer(PlayerControllerB player)
        {
            player.HealClientRpc();
        }

        
    }

}
