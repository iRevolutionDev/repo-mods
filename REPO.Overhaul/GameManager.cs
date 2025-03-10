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

            if (!PhotonNetwork.IsMasterClient) return;

            if (SemiFunc.RunIsLevel() && previousLevel == RunManager.instance.levelShop)
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    var playerAvatar = PlayerUtils.FindPlayerAvatar(player);
                    if (playerAvatar == null) continue;

                    if (!_previousPlayerHealth.TryGetValue(playerAvatar, out var health)) continue;

                    StatsManager.instance.SetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar), health, false);

                    _previousPlayerHealth.Remove(playerAvatar);

                    if (!SemiFunc.IsMultiplayer()) continue;

                    var maxHealth = PlayerUtils.GetPlayerMaxHealth(playerAvatar);
                    playerAvatar.photonView.RPC("UpdateHealthRPC", RpcTarget.Others, health, maxHealth, false);
                }
            }

            if (level != RunManager.instance.levelShop) return;

            if (SemiFunc.IsMultiplayer())
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    var playerAvatar = PlayerUtils.FindPlayerAvatar(player);

                    if (!PlayerUtils.IsPlayerAlive(playerAvatar)) continue;

                    SavePlayerHealth(playerAvatar);
                }
            }
            else
            {
                SavePlayerHealth(PlayerAvatar.instance);
            }
        }

        private void SavePlayerHealth(PlayerAvatar player)
        {
            if (player == null) return;

            var health = SemiFunc.IsMultiplayer()
                ? PlayerUtils.GetPlayerHealth(player)
                : PlayerUtils.GetLocalPlayerHealth();

            _previousPlayerHealth[player] = health;
        }
    }
}