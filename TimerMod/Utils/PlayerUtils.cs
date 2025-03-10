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
    }
}