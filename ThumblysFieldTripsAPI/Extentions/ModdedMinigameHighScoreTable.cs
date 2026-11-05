using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using ThumblysFieldTripsAPI.Registers;
using TMPro;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Extentions
{
    public class ModdedMinigameHighScoreTable : MonoBehaviour
    {
        private static FieldInfo _nameTmp = AccessTools.Field(typeof(MinigameHighScoreTable), "nameTmp");
        private static FieldInfo _valueTmp = AccessTools.Field(typeof(MinigameHighScoreTable), "valueTmp");
        private static FieldInfo _titleTmp = AccessTools.Field(typeof(MinigameHighScoreTable), "titleTmp");

        private bool initialized = false;

        public TMP_Text[] nameTmp = new TMP_Text[0];
        public TMP_Text[] valueTmp = new TMP_Text[0];
        public TMP_Text titleTmp;

        public void Setup(MinigameHighScoreTable highScoreTable)
        {
            if (initialized) return;
            initialized = true;
            nameTmp = (TMP_Text[])_nameTmp.GetValue(highScoreTable);
            valueTmp = (TMP_Text[])_valueTmp.GetValue(highScoreTable);
            titleTmp = (TMP_Text)_titleTmp.GetValue(highScoreTable);
        }

        public void UpdateScores(MinigameName minigame, string nameKey)
        {
            string enumName = EnumExtensions.GetExtendedName<MinigameName>((int)minigame);

            MinigameMetadata minigameMeta = MinigameMetaStorage.Instance.Get(minigame);

            titleTmp.text = Singleton<LocalizationManager>.Instance.GetLocalizedText(nameKey) + " " + Singleton<LocalizationManager>.Instance.GetLocalizedText("Mgm_Leaderboard");
            List<FieldTripScore> sortedScores = Singleton<ModdedFieldTripScoreManager>.Instance.GetSortedScoresForMinigame(enumName, minigameMeta.flags.HasFlag(MinigameFlags.TimeScoring));

            bool isTimed = minigameMeta.flags.HasFlag(MinigameFlags.TimeScoring);

            for (int i = 0; i < nameTmp.Length; i++)
            {
                if (sortedScores != null && i < sortedScores.Count)
                {
                    nameTmp[i].text = sortedScores[i].name;
                    if (isTimed)
                    {
                        float currentScore = (float)sortedScores[i].score;
                        int seconds = Mathf.FloorToInt(currentScore % 60f);
                        int minutes = Mathf.FloorToInt(currentScore / 60f);
                        int miliseconds = Mathf.FloorToInt((currentScore % 1) * 1000);
                        TMP_Text text = valueTmp[i];
                        text.alignment = TextAlignmentOptions.Right;
                        text.enableWordWrapping = false;
                        text.text = string.Format(FieldTripsAPI.TimeScoreFormat, minutes, seconds,miliseconds);
                    }
                    else
                    {
                        valueTmp[i].text = sortedScores[i].score.ToString();
                    }
                }
                else
                {
                    nameTmp[i].text = Singleton<LocalizationManager>.Instance.GetLocalizedText("Name_Baldi");
                    if (isTimed)
                    {
                        TMP_Text text = valueTmp[i];
                        text.alignment = TextAlignmentOptions.Right;
                        text.enableWordWrapping = false;
                        text.text = string.Format(FieldTripsAPI.TimeScoreFormat, "00", "00", "000");
                    }
                    else
                    {
                        valueTmp[i].text = "0";
                    }
                }
            }
        }

        private string Format00(int number)
        {
            if (number.ToString().Length == 1)
            {
                return "0" + number.ToString();
            }
            return number.ToString();
        }
    }
}
