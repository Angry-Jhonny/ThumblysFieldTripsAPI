using HarmonyLib;
using ThumblysFieldTripsAPI.Registers;

namespace ThumblysFieldTripsAPI.Patches
{
    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("ResetTrip")]
    internal class HighScoreManagerPatch
    {
        public static void Prefix()
        {
            Singleton<ModdedFieldTripScoreManager>.Instance.SetDefaultsTrips();
            Singleton<ModdedFieldTripScoreManager>.Instance.Save();
        }
    }
}
