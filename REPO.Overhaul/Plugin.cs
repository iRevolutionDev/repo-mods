using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using REPO.Overhaul.Patches;

namespace REPO.Overhaul
{
    [BepInPlugin("com.revolution.repo.overhaul", "REPO Overhaul", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony("com.revolution.repo.overhaul");

        public void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("REPO.Overhaul");

            _harmony.PatchAll(typeof(ChangeLevelPatch));
        }
    }
}