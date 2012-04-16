using System;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class Counter
    {
        public Counter()
        {
            Total = 0;
            Enablad = 0;
            Answered = 0;
        }
        public int Total { get; set; }
        public int Enablad { get; set; }
        public int Answered { get; set; }
        public int Progress
        {
            get
            {
                if (Enablad < 1)
                    return 0;
                return (int)Math.Round(100 * ((double)Answered / Enablad));
            }
        }

        public static Counter operator +(Counter a, Counter b)
        {
            return new Counter
                       {
                           Total = a.Total + b.Total,
                           Enablad = a.Enablad + b.Enablad,
                           Answered = a.Answered + b.Answered
                       };
        }
    }
}