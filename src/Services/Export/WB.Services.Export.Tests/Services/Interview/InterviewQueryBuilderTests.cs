using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using NUnit.Framework;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Services.Interview
{
    [TestOf(typeof(InterviewQueryBuilder))]
    [UseApprovalSubdirectory("approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    public class InterviewQueryBuilderTests
    {
        [Test]
        public void should_build_query_for_group_with_question()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.gA, 6, "quest", Create.Group(Id.gB, "group1",
                Create.NumericIntegerQuestion(variable: "num1")));
            questionnaire.ConnectChildrenWithParent();

            // Act
            var query = InterviewQueryBuilder.GetInterviewsQuery(Create.Tenant(name: "tenant"), questionnaire.Find<Group>(Id.gB));

            // Assert
            Approvals.Verify(query);
        }
    }
}
