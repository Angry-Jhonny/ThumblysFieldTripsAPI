using MTM101BaldAPI.Components.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThumblysFieldTripsAPI.Helpers
{
    internal class TutorialCustomImageAnimator : CustomImageAnimator
    {
        public AnimationEventTriggerCallFunctionInParentObjectWhyIsThisNecessaryHahaha animationEventTriggerCall;

        protected override void OnAnimationFinished()
        {
            animationEventTriggerCall.AnimationEvent();
            base.OnAnimationFinished();
        }
    }
}
