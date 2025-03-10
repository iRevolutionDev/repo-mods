using System.Linq;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace TimerMod.Utils
{
    public static class PlayerUtils
    {
        public static bool IsPlayerAlive(PlayerAvatar player)
        {
            if (player == null) return false;

            var health = StatsManager.instance.GetPlayerHealth(SemiFunc.PlayerGetSteamID(player));

            return health > 0;
        }

        public static int GetLocalPlayerHealth()
        {
            var healthField = AccessTools.Field(typeof(PlayerHealth), "health");

            return healthField.GetValue(PlayerAvatar.instance.playerHealth) as int? ?? 0;
        }

        public static int GetPlayerHealth(PlayerAvatar player)
        {
            return StatsManager.instance.GetPlayerHealth(SemiFunc.PlayerGetSteamID(player));
        }

        public static int GetPlayerMaxHealth(PlayerAvatar player)
        {
            return StatsManager.instance.GetPlayerMaxHealth(SemiFunc.PlayerGetSteamID(player));
        }

        public static PlayerAvatar FindPlayerAvatar(Player player)
        {
            return (from playerAvatar in Object.FindObjectsOfType<PlayerAvatar>()
                let component = playerAvatar.GetComponent<PhotonView>()
                where component != null && component.Owner.Equals(player)
                select playerAvatar).FirstOrDefault();
        }
    }
}