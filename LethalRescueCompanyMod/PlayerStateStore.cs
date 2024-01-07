using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalRescueCompanyMod
{
    public static class PlayerStateStore
    {
        public static PlayerControllerB? playerControllerB => GameNetworkManager.Instance.localPlayerController;
        public static DeadBodyInfo? deadBodyInfo => playerControllerB?.deadBody;
        public static StartOfRound? playersManager => playerControllerB?.playersManager;
        public static bool isDebug = Settings.isDebug;
    }
}
