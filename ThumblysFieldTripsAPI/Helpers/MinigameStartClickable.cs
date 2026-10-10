using System;
using System.Collections.Generic;
using System.Text;

namespace ThumblysFieldTripsAPI.Helpers
{
    using ThumblysFieldTripsAPI.Extentions;
    using UnityEngine;
    using UnityEngine.Events;

    public class MinigameStartClickable : MonoBehaviour, IClickable<int>
    {
        public FieldTripBaseRoomFunction roomFunction;

        public MinigameBase minigame;

        public void Clicked(int playerNumber)
        {
            roomFunction.StartMinigame(minigame);
        }
        public void ClickableSighted(int player) { }
        public void ClickableUnsighted(int player) { }
        public bool ClickableHidden() => false;
        public bool ClickableRequiresNormalHeight() => true;
    }
}
