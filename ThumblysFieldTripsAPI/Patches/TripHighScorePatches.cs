using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using System;
using System.Reflection;
using ThumblysFieldTripsAPI.Extentions;
using ThumblysFieldTripsAPI.Registers;

namespace ThumblysFieldTripsAPI.Patches
{
    [HarmonyPatch(typeof(MinigameHighScoreTable))]
    [HarmonyPatch("UpdateScores")]
    [HarmonyPatch(new Type[] { typeof(MinigameName), typeof(string) })]
    internal class MinigameHighScoreTablePatch
    {
        public static bool Prefix(MinigameHighScoreTable __instance, MinigameName minigame, string nameKey)
        {
            if (__instance.TryGetComponent<ModdedMinigameHighScoreTable>(out ModdedMinigameHighScoreTable highScoreTable))
            {
                highScoreTable.UpdateScores(minigame, nameKey);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HighScoreManager))]
    [HarmonyPatch("AddTripScore")]
    internal class MinigameBaseScorePatch
    {
        public static bool Prefix(int score, string name, MinigameName minigame, bool greaterWins, out int rank)
        {
            rank = -1;
            if (Enum.GetName(typeof(MinigameName), (int)minigame) == null)
            {
                Singleton<ModdedFieldTripScoreManager>.Instance.AddScore(EnumExtensions.GetExtendedName<MinigameName>((int)minigame), score, name, out rank);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("ResetTrip")]
    internal class ResetTripPatch
    {
        public static void Prefix()
        {
            Singleton<ModdedFieldTripScoreManager>.Instance.SetDefaultsTrips();
            Singleton<ModdedFieldTripScoreManager>.Instance.Save();
        }
    }
}
