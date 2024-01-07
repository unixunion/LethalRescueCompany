using GameNetcodeStuff;
using LethalRescueCompanyMod.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class ReviveStore : NetworkBehaviour
    {
        public static ReviveStore instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            instance = this;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestReviveServerRpc(ulong playerId)
        {
            ReviveScript reviveScript = GetComponent<ReviveScript>();
            if (reviveScript != null)
            {
                reviveScript.TryRevivePlayer(playerId);
            }
        }

    }

    internal class ReviveScript : NetworkBehaviour
    {
        public void TryRevivePlayer(ulong playerId)
        {
            if (!IsServer) return;

            PlayerControllerB player = GetPlayerById(playerId);
            if (player == null || player.health != 0) return;

            RevivePlayer(player);
        }

        private void RevivePlayer(PlayerControllerB player)
        {
            player.HealClientRpc();
        }

        private PlayerControllerB GetPlayerById(ulong playerId)
        {
            PlayerControllerB[] players = ReviveHelpers.Players;
            List<PlayerControllerB> list = new List<PlayerControllerB>();
            PlayerControllerB[] array = players;
            foreach (PlayerControllerB val in array)
            {
                if (val.isPlayerDead)
                {
                    list.Add(val);
                }
            }
            foreach (PlayerControllerB item in list)
            {
                if (item.playerClientId == playerId)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
