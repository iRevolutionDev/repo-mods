using HarmonyLib;

namespace TimerMod.Patches
{
    [HarmonyPatch(typeof(GameDirector), "Update")]
    public static class GameDirectorPatch
    {
        public static void Postfix(GameDirector __instance)
        {
            if (__instance == null) return;

            TimerManager.Instance.OnGameDirectorUpdate(__instance);
        }
    }
}