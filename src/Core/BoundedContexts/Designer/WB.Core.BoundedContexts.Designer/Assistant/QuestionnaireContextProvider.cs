namespace WB.Core.BoundedContexts.Designer.Assistant;

public class QuestionnaireContextProvider : IQuestionnaireContextProvider
{
    public string GetQuestionnaireContext(string questionnaireId)
    {
        //Class generation
        //could be too large for complex questionnaires
        //some referenced entities could be missing
            
        //var questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
        // var questionnaire = this.GetQuestionnaire(id).Source;
        // var supervisorVersion = version ?? this.engineVersionService.LatestSupportedVersion;
        // var package = generationPackageFactory.Generate(questionnaire);
        // var generated = this.expressionProcessorGenerator.GenerateProcessorStateClasses(package, supervisorVersion, inSingleFile: true);
        //
        // var resultBuilder = new StringBuilder();
        //
        // foreach (KeyValuePair<string, string> keyValuePair in generated)
        // {
        //     resultBuilder.AppendLine(string.Format("//{0}", keyValuePair.Key));
        //     resultBuilder.AppendLine(keyValuePair.Value);
        // }
            
            
        //Consider location of assistant call as a context to extract relevant parts of questionnaire
        //traverse questionnaire structure to the root and extract relevant parts
        //current code generation is lacking the description of entities like question texts
            
        // Placeholder implementation
        return $"Context for questionnaire {questionnaireId}";
            
    }
}
