using UnityEngine;

namespace ThumblysFieldTripsAPI
{
    /// <summary>
    /// Wait for the defined amount of time depending on the DeltaTime of the MinigameBase.
    /// </summary>
    public class WaitForSecondsMinigameTimescale : CustomYieldInstruction
    {
        public MinigameBase minigameBase;

        public float timeRemaining;

        public override bool keepWaiting
        {
            get
            {
                if (!minigameBase)
                {
                    return false;
                }

                timeRemaining -= minigameBase.DeltaTime;
                return timeRemaining >= 0f;
            }
        }

        public WaitForSecondsMinigameTimescale(MinigameBase minigame, float seconds)
        {
            timeRemaining = seconds;
            minigameBase = minigame;
        }
    }
}