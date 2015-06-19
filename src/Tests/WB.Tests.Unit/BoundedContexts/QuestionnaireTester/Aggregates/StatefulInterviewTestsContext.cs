using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    [Subject(typeof(StatefulInterview))]
    public class StatefulInterviewTestsContext
    {
        protected static void SetupQuestionnaireWithLinkedAndReferencedQuestions(Guid questionnaireId,
            Guid linkedQuestionId, Guid[] linkedQuestionRosters, Guid referencedQuestionId, Guid[] referencedQuestionRosters)
        {
            Setup.QuestionnaireWithRepositoryToMockedServiceLocator(questionnaireId, _
                => _.HasQuestion(linkedQuestionId) == true
                && _.GetRosterLevelForQuestion(linkedQuestionId) == linkedQuestionRosters.Length
                && _.GetRostersFromTopToSpecifiedQuestion(linkedQuestionId) == linkedQuestionRosters
                && _.HasQuestion(referencedQuestionId) == true
                && _.GetRosterLevelForQuestion(referencedQuestionId) == referencedQuestionRosters.Length
                && _.GetRostersFromTopToSpecifiedQuestion(referencedQuestionId) == referencedQuestionRosters);
        }
    }
}