namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    /// <summary>
    /// 
    /// </summary>
    public class FlowRule
    {
        public string ConditionExpression { set; get; }
        public SurveyStatus StatusToGo { set; get; }
        public bool Enabled { set; get; }
        public string ChangeComment { set; get; }
    }
}
