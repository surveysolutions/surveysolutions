using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal interface IQuestionDetailsFactory
    {
        QuestionDetailsView CreateQuestion(IQuestion question);
    }
}