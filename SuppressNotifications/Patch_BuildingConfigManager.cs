﻿using Harmony;
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
            ___baseTemplate.AddComponent<StatusItemsSuppressedComp>();
            ___baseTemplate.AddComponent<NotificationsSuppressedComp>();
            ___baseTemplate.AddComponent<BuildingNotificationButton>();
        }
    }
}
