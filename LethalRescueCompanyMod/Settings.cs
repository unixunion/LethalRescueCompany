using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalRescueCompanyMod
{
    /**
     * The accessor for the settings, maybe move this to pluginConfig...
     **/
    public static class Settings
    {
        public static bool isDebug { get; set; } = true;
        public static bool isSolo { get; set; } = false;

    }
}
