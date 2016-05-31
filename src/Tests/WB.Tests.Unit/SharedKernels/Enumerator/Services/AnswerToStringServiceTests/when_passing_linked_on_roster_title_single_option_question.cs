using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.AnswerToStringServiceTests
{
    internal class when_passing_linked_on_roster_title_single_option_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            answerToStringService = CreateAnswerToStringService();

            var rosterSourceOfLinking = CreateInterviewRoster("nastya", Create.RosterVector(3));
            statefulInterviewMock = Substitute.For<IStatefulInterview>();
            statefulInterviewMock.FindReferencedRostersForLinkedQuestion(rosterId, Arg.Any<Identity>())
                .Returns(new[] { rosterSourceOfLinking });

            singleOptionAnswer = CreateLinkedSingleOptionAnswer(Create.RosterVector(3), linkedId);

            questionnaire = Create.PlainQuestionnaire(
                Create.QuestionnaireDocumentWithOneChapter(
                    Create.Roster(rosterId: rosterId, children: new IComposite[]
                    {
                        Create.SingleOptionQuestion(questionId: linkedId, linkedToRosterId:rosterId, variable:"link"),
                        Create.TextQuestion(questionId:questionWithSubstitutionId, text:"test %link%")
                    })));
        };

        Because of = () =>
            result = answerToStringService.AnswerToUIString(linkedId, singleOptionAnswer, statefulInterviewMock, questionnaire);

        It should_return_linked_roster_title = () =>
            result.ShouldEqual("nastya");

        static string result;
        static IQuestionnaire questionnaire;
        static LinkedSingleOptionAnswer singleOptionAnswer;
        static IAnswerToStringService answerToStringService;
        static Guid questionWithSubstitutionId = Id.g1;
        static Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid linkedId = Id.g2;
        static IStatefulInterview statefulInterviewMock;
    }
}