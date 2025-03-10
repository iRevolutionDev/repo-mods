using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimerMod.Patches;

namespace TimerMod
{
    [BepInPlugin("com.revolution.timermod", "Timer Mod", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony("com.revolution.timermod");

        public void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("TimerMod");

            _harmony.PatchAll(typeof(ChangeLevelPatch));
            _harmony.PatchAll(typeof(ExtractionPatch));
            _harmony.PatchAll(typeof(GameDirectorPatch));
            _harmony.PatchAll(typeof(MenuPagePatch));
        }
    }
}