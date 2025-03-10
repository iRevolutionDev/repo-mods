using System.Linq;
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

        public static int GetPlayerHealth(PlayerAvatar player)
        {
            return StatsManager.instance.GetPlayerHealth(SemiFunc.PlayerGetSteamID(player));
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