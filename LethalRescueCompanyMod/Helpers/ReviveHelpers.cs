using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalRescueCompanyMod.Helpers
{
    public static partial class ReviveHelpers
    {
        public static PlayerControllerB LocalPlayer => GameNetworkManager.Instance.localPlayerController;

        public static PlayerControllerB[] Players => ReviveHelpers.StartOfRound?.allPlayerScripts;

        public static PlayerControllerB GetPlayer(string playerNameOrId)
        {
            PlayerControllerB[] players = ReviveHelpers.Players;

            return players?.FirstOrDefault(player => player.playerUsername == playerNameOrId) ??
                   players?.FirstOrDefault(player => player.playerClientId.ToString() == playerNameOrId);
        }

        public static PlayerControllerB GetPlayer(int playerId) => ReviveHelpers.GetPlayer(playerId.ToString());
        public static StartOfRound StartOfRound => StartOfRound.Instance;
    }
}
