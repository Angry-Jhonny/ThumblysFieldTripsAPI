using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Extentions
{
    public static class MinigameHUDExtentions
    {
        internal readonly static FieldInfo _scoreTmp = AccessTools.Field(typeof(MinigameHUD), "scoreTmp");


        public static void SetTime(this MinigameHUD HUD, int time)
        {
            float currentScore = time / 100f;
            int seconds = Mathf.FloorToInt(currentScore % 60f);
            int minutes = Mathf.FloorToInt(currentScore / 60f);
            int miliseconds = Mathf.FloorToInt(currentScore % 1 * 100);
            ((TMP_Text)_scoreTmp.GetValue(HUD)).text = string.Format(FieldTripsAPI.TimeScoreFormat, Format00(minutes), Format00(seconds), Format00(miliseconds));
            HUD.StopAllCoroutines();
        }

        private static string Format00(int number)
        {
            if (number.ToString().Length == 1)
            {
                return "0" + number.ToString();
            }
            return number.ToString();
        }
    }
}
