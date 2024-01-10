using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class SpawnUtils : NetworkBehaviour
    {
        public void CloneDeadBody(DeadBodyInfo deadBodyInfo)
        {

            // copy whats important
            var rd = deadBodyInfo.playerScript.playersManager.playerRagdolls[0];
            var tf = deadBodyInfo.transform;

            // server | client check?
            Destroy(deadBodyInfo);

            GameObject gameObject = Instantiate(rd,tf.position,tf.rotation);
            if (IsServer) gameObject.GetComponent<NetworkObject>().Spawn();

        }



    }
}
