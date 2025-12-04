using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireDocumentTransformer
    {
        QuestionnaireDocument TransformInPlace(QuestionnaireDocument document);
    }
}
