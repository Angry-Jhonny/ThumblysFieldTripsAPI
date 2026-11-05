using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumblysFieldTripsAPI
{
    public enum ScoringType
    {
        SlowestTime,
        FastestTime,
        HighestScore,
        LowestScore,
        [Obsolete("please use SlowestTime")]
        HighestTime = SlowestTime,
        [Obsolete("please use FastestTime")]
        LowestTime = FastestTime
    }

    public static class ScoringTypeExtentions
    {
        public static bool GetWinsType(this ScoringType type)
        {
            bool flag = false;
            switch (type)
            {
                case ScoringType.HighestScore:
                    flag = true; break;
                case ScoringType.SlowestTime:
                    flag = true; break;
            }
            return flag;
        }

        public static bool IsTimeScoring(this ScoringType type)
        {
            bool flag = false;
            switch (type)
            {
                case ScoringType.SlowestTime:
                    flag = true; break;
                case ScoringType.FastestTime:
                    flag = true; break;
            }
            return flag;
        }
    }
}
