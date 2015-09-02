using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class TextListQuestionChanged : AbstractListQuestionDataEvent
    {
        public QuestionScope QuestionScope { get; set; }
    }
}
