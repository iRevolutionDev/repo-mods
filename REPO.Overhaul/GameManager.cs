using System.Collections.Generic;
using Photon.Pun;
using TimerMod.Utils;

namespace REPO.Overhaul
{
    public class GameManager
    {
        private static GameManager _instance;

        private readonly Dictionary<PlayerAvatar, int> _previousPlayerHealth = new Dictionary<PlayerAvatar, int>();
        public static GameManager Instance => _instance ?? (_instance = new GameManager());

        public void OnLevelChange(Level level, Level previousLevel)
        {
            if (level == null) return;

            if (SemiFunc.RunIsLevel() && previousLevel == RunManager.instance.levelShop)
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    var playerAvatar = PlayerUtils.FindPlayerAvatar(player);
                    if (playerAvatar == null) continue;

                    if (!_previousPlayerHealth.TryGetValue(playerAvatar, out var health)) continue;

                    if (!PhotonNetwork.IsMasterClient) continue;

                    PlayerUtils.GetPlayerHealth(playerAvatar);
                    StatsManager.instance.SetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar), health, false);
                }
            }

            if (level != RunManager.instance.levelShop || !PhotonNetwork.IsMasterClient) return;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                var playerAvatar = PlayerUtils.FindPlayerAvatar(player);
                if (playerAvatar == null) continue;

                SavePlayerHealth(playerAvatar);
            }
        }

        private void SavePlayerHealth(PlayerAvatar player)
        {
            if (player == null) return;

            var health = PlayerUtils.GetPlayerHealth(player);
            _previousPlayerHealth[player] = health;
        }
    }
}