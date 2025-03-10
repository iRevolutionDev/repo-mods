using HarmonyLib;

namespace TimerMod.Patches
{
    [HarmonyPatch(typeof(ExtractionPoint), "ButtonPress")]
    public static class ExtractionPatch
    {
        public static void Postfix()
        {
            if (!SemiFunc.RunIsLevel()) return;

            TimerManager.Instance.OnExtraction();
        }
    }
}