using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class StaticTextUpdated : QuestionnaireEntityEvent
    {  
        public string Text { get; set; }
    }
}
