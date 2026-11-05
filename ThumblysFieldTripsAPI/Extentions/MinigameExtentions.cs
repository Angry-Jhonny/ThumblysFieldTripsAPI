using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThumblysFieldTripsAPI.Extentions
{
    public static class MinigameExtentions
    {
        private static FieldInfo _minigame = AccessTools.Field(typeof(MinigameBase), "minigame");
        internal readonly static FieldInfo _scoreTmp = AccessTools.Field(typeof(MinigameHUD), "scoreTmp");
        internal readonly static FieldInfo _disableOnPause = AccessTools.Field(typeof(MinigameBase), "disableOnPause");

        public static MinigameName GetMinigameEnum(this Minigame minigame) => (MinigameName)_minigame.GetValue(minigame.GetComponent<MinigameBase>());
        public static MinigameName GetMinigameEnum(this MinigameBase minigameBase) => (MinigameName)_minigame.GetValue(minigameBase);

        public static void AddDisableOnPauseObject(this MinigameBase minigameBase, GameObject obj)
        {
            GameObject[] list = (GameObject[])_disableOnPause.GetValue(minigameBase);
            list = list.AddToArray(obj);
            _disableOnPause.SetValue(minigameBase, list);
        }

        /// <summary>
        /// Sets the time on the screen.
        /// </summary>
        public static void SetTime(this MinigameHUD HUD, float time)
        {
            int seconds = Mathf.FloorToInt(time % 60f);
            int minutes = Mathf.FloorToInt(time / 60f);
            int miliseconds = Mathf.FloorToInt(time % 1 * 1000);
            ((TMP_Text)_scoreTmp.GetValue(HUD)).text = string.Format(FieldTripsAPI.TimeScoreFormat, minutes, seconds, miliseconds);
            HUD.StopAllCoroutines();
        }

        /// <summary>
        /// Adds a clock on the screen, similar to a gauge.
        /// </summary>
        public static void SetCountdown(this MinigameHUD HUD, float countdown, float totalCountdown)
        {
            if (HUD.GetComponent<MinigameCountdown>() != null)
            {
                HUD.GetComponent<MinigameCountdown>().SetCountdown(countdown, totalCountdown);
            }
        }

        // boohoo not useful :sob:
        /*public static void AddCountdown(this MinigameHUD HUD)
        {
            if (HUD.GetComponent<MinigameCountdown>() != null)
            {
                HUD.GetComponent<MinigameCountdown>().dialImage.enabled = true;
            }
        }*/
    }

    public class MinigameCountdown : MonoBehaviour
    {
        public Sprite[] dialSprite = new Sprite[0];
        public Image dialImage;
        private float targetValue;
        public void SetCountdown(float countdown, float totalCountdown)
        {
            if (countdown <= 0f)
            {
                dialImage.enabled = false;
                return;
            }
            else { dialImage.enabled = true; }
            targetValue = Mathf.Clamp01(countdown / totalCountdown);
            dialImage.sprite = dialSprite[Mathf.RoundToInt((1f - targetValue) * (float)(dialSprite.Length - 1))];
        }
    }
}
