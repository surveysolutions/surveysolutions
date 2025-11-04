using Main.Core.Documents;

public interface IQuestionnaireDocumentTransformer
{
    QuestionnaireDocument TransformDocument(QuestionnaireDocument document);
}
