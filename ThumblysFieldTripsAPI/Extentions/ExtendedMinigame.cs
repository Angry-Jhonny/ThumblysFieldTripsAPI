using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumblysFieldTripsAPI.Extentions
{
    public class ExtendedMinigame : Minigame
    {
        public float ElapsedTime { get; protected set; }

        public override void VirtualReset()
        {
            ElapsedTime = 0f;
        }

        protected virtual void SetTime(float time)
        {
            ElapsedTime += time;
        }
    }
}
