namespace WB.WebInterview.Stress
{
    public class Configuration
    {
        public string startUri { get; set; }
        public int workersCount { get; set; }
        public int restartWorkersIn { get; set; } = 30000;
        public int answerDelay { get; set; } = 1000;
        public int createInterviewDelay { get; set; } = 30000;
        public double shareInterviewPropability { get; set; } = 0.5;
    }
}