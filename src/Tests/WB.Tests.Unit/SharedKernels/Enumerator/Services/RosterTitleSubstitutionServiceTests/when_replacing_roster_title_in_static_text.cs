using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.RosterTitleSubstitutionServiceTests
{
    internal class when_replacing_roster_title_in_static_text : RosterTitleSubstitutionServiceTestsContext
    {
        Establish context = () =>
        {
            staticTextId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitle = "rosterValue";

            var interview = Mock.Of<IStatefulInterview>(x =>
                x.QuestionnaireIdentity == questionnaireIdentity &&
                x.FindRosterByOrDeeperRosterLevel(Moq.It.IsAny<Guid>(), Moq.It.IsAny<RosterVector>()) == new InterviewRoster { Title = rosterTitle });

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.Roster(rosterId: Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), children: new[]
                    {
                        Create.Entity.StaticText(publicKey: staticTextId)
                    })));

            var questionnaireStorageStub = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaire);

            var interviewRepositoryStub = new Mock<IStatefulInterviewRepository>();
            interviewRepositoryStub.SetReturnsDefault(interview);

            service = new RosterTitleSubstitutionService(questionnaireStorageStub, interviewRepositoryStub.Object, Create.Service.SubstitutionService());
        };

        Because of = () => substitutedValue = service.Substitute("something %rostertitle%", new Identity(staticTextId, new decimal[] { 1 }), "interviewId");

        It should_replace_roster_title_with_value = () => substitutedValue.ShouldEqual($"something {rosterTitle}");

        static IRosterTitleSubstitutionService service;
        static string rosterTitle;
        static string substitutedValue;
        static Guid staticTextId;
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), 7);
    }
}