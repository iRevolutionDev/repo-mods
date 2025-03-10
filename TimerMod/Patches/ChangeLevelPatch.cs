using HarmonyLib;

namespace TimerMod.Patches
{
    [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
    public static class ChangeLevelPatch
    {
        public static void Postfix(RunManager __instance, Level ___levelCurrent)
        {
            TimerManager.Instance.OnLevelChange(___levelCurrent);
        }
    }
}