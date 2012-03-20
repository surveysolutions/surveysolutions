namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    /// <summary>
    /// Is used for holding rule of status changing.
    /// </summary>
    public class FlowRule
    {
        public string ConditionExpression { set; get; }
        public SurveyStatus TargetStatus { set; get; }
        public bool Enabled { set; get; }
        public string ChangeComment { set; get; }

        public string StatusId { set; get; }
    }
}
