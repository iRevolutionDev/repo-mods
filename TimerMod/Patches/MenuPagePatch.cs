using HarmonyLib;

namespace TimerMod.Patches
{
    [HarmonyPatch(typeof(MenuPage), "LockAndHide")]
    public static class MenuPagePatch
    {
        public static void Postfix()
        {
            if (!SemiFunc.RunIsLevel()) return;

            TimerManager.Instance.HideTimerText();
        }
    }
}