namespace WB.WebInterview.Stress
{
    public class Configuration
    {
        public string amountOfQuestionsToAnswer { get; set; } = "10-20";

        public (int? From, int To) questionsToAnswerRange
        {
            get
            {
                var range = amountOfQuestionsToAnswer.Split('-');
                if (range.Length > 1)
                {
                    return (int.Parse(range[0]), int.Parse(range[1]));
                }

                return (null, int.Parse(range[0]));
            }
        }
        public string startUri { get; set; }
        public int workersCount { get; set; } = 8;
        public int restartWorkersIn { get; set; } = 30000;
        public int answerDelay { get; set; } = 1000;
        public int createInterviewDelay { get; set; } = 30000;
        public double shareInterviewPropability { get; set; } = 0.5;
    }
}
