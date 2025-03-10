using HarmonyLib;

namespace REPO.Overhaul.Patches
{
    [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
    public static class ChangeLevelPatch
    {
        public static void Prefix(Level ___levelCurrent, Level ___levelPrevious)
        {
            GameManager.Instance.OnLevelChange(___levelCurrent, ___levelPrevious);
        }
    }
}