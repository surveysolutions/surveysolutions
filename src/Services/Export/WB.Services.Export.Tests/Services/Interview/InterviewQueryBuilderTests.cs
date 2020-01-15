using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
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
        public void should_not_include_static_text_in_data_part_of_query()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.gA, 6, "quest",
                Create.Group(Id.gB, 
                    variable: "gr1",
                    children: new IQuestionnaireEntity[]
                    {
                        Create.NumericIntegerQuestion(Id.gE, variable: "numeric1"),
                        Create.StaticText(Id.gC)
                    }));
            questionnaire.ConnectChildrenWithParent();

            // Act
            var query = InterviewQueryBuilder.GetInterviewsQuery(questionnaire.Find<Group>(Id.gB));

            // Assert
            Approvals.Verify(query);
        }

        [Test]
        public void should_build_query_for_group_with_question()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.gA, 6, "quest", Create.Group(Id.gB, "group1",
                Create.NumericIntegerQuestion(variable: "num1")));
            questionnaire.ConnectChildrenWithParent();

            // Act
            var query = InterviewQueryBuilder.GetInterviewsQuery(questionnaire.Find<Group>(Id.gB));

            // Assert
            Approvals.Verify(query);
        }

        [Test]
        public void should_be_able_to_build_query_for_roster()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.gA, 6, "quest", 
                Create.Roster(Id.gB, variable: "roster1",
                    children: new IQuestionnaireEntity[] { Create.NumericIntegerQuestion(variable: "num1") }));
            questionnaire.ConnectChildrenWithParent();

            // Act
            var query = InterviewQueryBuilder.GetInterviewsQuery(questionnaire.Find<Group>(Id.gB));

            // Assert
            Approvals.Verify(query);
        }

        [Test]
        public void should_be_able_to_build_query_for_empty_roster()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.gA, 6, "quest", 
                Create.Roster(Id.gB, variable: "roster1"));
            questionnaire.ConnectChildrenWithParent();

            // Act
            var query = InterviewQueryBuilder.GetInterviewsQuery(questionnaire.Find<Group>(Id.gB));

            // Assert
            Approvals.Verify(query);
        }

        [Test]
        public void should_be_able_to_build_query_for_group_with_static_text()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.gA, 6, "quest", 
                Create.Group(Id.gB, variable: "gr1", Create.StaticText(Id.gC)));
            questionnaire.ConnectChildrenWithParent();

            // Act
            var query = InterviewQueryBuilder.GetInterviewsQuery(questionnaire.Find<Group>(Id.gB));

            // Assert
            Approvals.Verify(query);
        }

        [Test]
        public void GetQuestionAnswersQuery_should_be_able_to_build_query_for_questions_on_top_level()
        {
            var questionnaireEntity = new MultimediaQuestion
            {
                QuestionType = QuestionType.Multimedia,
                VariableName = "mult1",
                PublicKey = Id.gC
            };
            Create.QuestionnaireDocumentWithOneChapter(
                variable: "questionnaire",
                chapterId: Id.gA,
                id: Id.gB,
                children: questionnaireEntity);

            // Act 
            var result = InterviewQueryBuilder.GetEnabledQuestionAnswersQuery(questionnaireEntity);

           // Assert
           Approvals.Verify(result);
        }

        [Test]
        public void GetQuestionAnswersQuery_should_be_able_to_build_query_for_questions_in_roster()
        {
            var questionnaireEntity = new MultimediaQuestion
            {
                QuestionType = QuestionType.Multimedia,
                VariableName = "mult1",
                PublicKey = Id.gC
            };

            Create.QuestionnaireDocumentWithOneChapter(
                variable: "questionnaire",
                chapterId: Id.gA,
                id: Id.gB,
                children: Create.Roster(variable: "rost", children: new List<IQuestionnaireEntity>
                {
                    questionnaireEntity
                }));

            // Act 
            var result = InterviewQueryBuilder.GetEnabledQuestionAnswersQuery(questionnaireEntity);

            // Assert
            Approvals.Verify(result);
        }
    }
}
