using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumblysFieldTripsAPI
{
    public enum ScoringType
    {
        HighestScore,
        HighestTime,
        LowestScore,
        LowestTime,
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
                case ScoringType.HighestTime:
                    flag = true; break;
            }
            return flag;
        }

        public static bool IsTimeScoring(this ScoringType type)
        {
            bool flag = false;
            switch (type)
            {
                case ScoringType.HighestTime:
                    flag = true; break;
                case ScoringType.LowestTime:
                    flag = true; break;
            }
            return flag;
        }
    }
}
