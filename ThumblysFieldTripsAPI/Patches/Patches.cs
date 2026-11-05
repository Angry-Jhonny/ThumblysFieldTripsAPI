using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Components.Animation;
using System;
using System.CodeDom;
using System.Collections;
using System.Linq;
using System.Reflection;
using ThumblysFieldTripsAPI.Extentions;
using ThumblysFieldTripsAPI.Helpers;
using ThumblysFieldTripsAPI.Registers;
using TMPro;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Patches
{
    [HarmonyPatch(typeof(FieldTripEntranceRoomFunction), "Initialize")]
    internal class EntranceRoomFunctionPatch
    {
        public static void Prefix(FieldTripEntranceRoomFunction __instance, RoomController room, ref Animator ___animator, ref SoundObject ___audCampIntro)
        {
            if (!CoreGameManager.Instance.tripPlayed && CoreGameManager.Instance.tripAvailable)
            {
                ___audCampIntro = CoreGameManager.Instance.currentFieldTrip.intro;
            }
        }
    }

    [HarmonyPatch(typeof(MinigameHighScoreTable), "UpdateScores")]
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

    [HarmonyPatch(typeof(MinigameBase), "SetScore")]
    internal class MinigameEndPatch
    {
        private static MethodInfo _ShowResults = AccessTools.Method(typeof(MinigameBase), "ShowResults", [typeof(bool), typeof(int), typeof(int)]);

        public static bool Prefix(MinigameName ___minigame, MinigameBase __instance, int score, ref bool ___minigameActive, ref bool ___result, string ___minigameNameKey)
        {
            if (Enum.GetName(typeof(MinigameName), (int)___minigame) == null) 
            {
                bool isTimeScoring = MinigameMetaStorage.Instance.Get(__instance.GetMinigameEnum()).flags.HasFlag(MinigameFlags.TimeScoring);
                Singleton<ModdedFieldTripScoreManager>.Instance.AddScore(___minigame.ToStringExtended(), isTimeScoring ? __instance.GetComponent<ExtendedMinigame>().ElapsedTime : score, Singleton<PlayerFileManager>.Instance.fileName, true, out var rank);
                if (rank >= 0)
                {
                    Singleton<HighlightManager>.Instance.Highlight(
                        "steam_trophy",
                        string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText("Steam_Highlight_MinigameHighScore"),
                            Singleton<LocalizationManager>.Instance.GetLocalizedText(___minigameNameKey)),
                        string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText("Steam_Highlight_MinigameHighScore_Desc"),
                            (rank + 1).ToString(),
                            isTimeScoring ? score : score, Singleton<LocalizationManager>.Instance.GetLocalizedText(___minigameNameKey)),
                        3u,
                        0f - __instance.RealTime, __instance.RealTime + 5f,
                        TimelineEventClipPriority.Featured);
                }

                ___minigameActive = false;
                _ShowResults.Invoke(__instance, [rank >= 0, rank, score]);
                ___result = rank >= 0;
                Singleton<MusicManager>.Instance.SetFileLoop(false);
                Singleton<MusicManager>.Instance.SetLoop(false);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MinigameBase), "ShowResults")]
    [HarmonyPatch(new Type[] { typeof(bool), typeof(int), typeof(int) })]
    internal class MinigameResultsPatch
    {
        public static bool Prefix(MinigameName ___minigame, MinigameBase __instance, bool result, int rank, int score, ref TMP_Text ___resultTmp, ref bool ___scoringMode, ref GameObject ___resultsObject)
        {
            if (Enum.GetName(typeof(MinigameName), (int)___minigame) == null)
            {
                if (___scoringMode)
                {
                    ___resultsObject.SetActive(true);
                    bool isTimeScoring = MinigameMetaStorage.Instance.Get(___minigame).flags.HasFlag(MinigameFlags.TimeScoring);
                    string formattedTime = "";
                    if (isTimeScoring)
                    {
                        float currentScore = __instance.GetComponent<ExtendedMinigame>().ElapsedTime;
                        int seconds = Mathf.FloorToInt(currentScore % 60f);
                        int minutes = Mathf.FloorToInt(currentScore / 60f);
                        int miliseconds = Mathf.FloorToInt((currentScore % 1) * 1000);
                        formattedTime = string.Format(FieldTripsAPI.TimeScoreFormat, minutes, seconds, miliseconds);
                    }

                    if (result)
                    {
                        ___resultTmp.text = string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText(isTimeScoring ? "Mgm_FinalTime" : "Mgm_FinalScore"), isTimeScoring ? formattedTime : score) + string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText("Mgm_HighScore"), rank + 1);
                    }
                    else
                    {
                        ___resultTmp.text = string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText(isTimeScoring ? "Mgm_FinalTime" : "Mgm_FinalScore"), isTimeScoring ? formattedTime : score) + Singleton<LocalizationManager>.Instance.GetLocalizedText("Mgm_LoseEndless");
                    }
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(OptionsMenu), "ResetTrip")]
    internal class ResetTripPatch
    {
        public static void Prefix()
        {
            Singleton<ModdedFieldTripScoreManager>.Instance.SetDefaultsTrips();
            Singleton<ModdedFieldTripScoreManager>.Instance.Save();
        }
    }

    [HarmonyPatch(typeof(MainMenu), "Start")]
    internal static class MainMenuPatch
    {
        static void Postfix(MainMenu __instance)
        {
            __instance.StartCoroutine(WaitForLoad());
        }

        static IEnumerator WaitForLoad()
        {
            while (Singleton<AdditiveSceneManager>.Instance.Busy)
                yield return null;

            Transform fieldTripScreen = Resources.FindObjectsOfTypeAll<CursorInitiator>().First(x => x.GetInstanceID() > 0 && x.name == "PickFieldTrip").transform;
            fieldTripScreen.gameObject.AddComponent<PickFieldTripManager>().BuildScreen();
        }
    }
}
