﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;

namespace SuppressNotifications
{
    // Patch to add the BuildingNotificationButton to all buildings that can be disabled
    // May need to be moved to suppress notifications on other buildings (ladders?)
    [HarmonyPatch(typeof(BuildingConfigManager), "OnPrefabInit")]
    class Patch_BuildingConfigManager
    {
        static void Postfix(ref GameObject ___baseTemplate)
        {
            Debug.Log(1);
            ___baseTemplate.AddComponent<StatusItemsSuppressed>();
            ___baseTemplate.AddComponent<BuildingNotificationButton>();
            Debug.Log(2);
        }
    }
}
