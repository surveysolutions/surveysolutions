using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.RosterTitleSubstitutionServiceTests
{
    internal class when_replacing_roster_title : RosterTitleSubstitutionServiceTestsContext
    {
        Establish context = () =>
        {
            questionid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitle = "rosterValue";

            var interview = Mock.Of<IStatefulInterview>(x => 
                x.QuestionnaireIdentity == questionnaireIdentity &&
                x.FindRosterByOrDeeperRosterLevel(Moq.It.IsAny<Guid>(), Moq.It.IsAny<RosterVector>()) == new InterviewRoster { Title = rosterTitle });

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetRostersFromTopToSpecifiedQuestion(questionid) == new [] { Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB") });

            var questionnaireStorageStub = Mock.Of<IPlainQuestionnaireRepository>(_
                => _.GetHistoricalQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version) == questionnaire);

            var interviewRepositoryStub = new Mock<IStatefulInterviewRepository>();
            interviewRepositoryStub.SetReturnsDefault(interview);

            service = new RosterTitleSubstitutionService(questionnaireStorageStub, interviewRepositoryStub.Object, Create.SubstitutionService());
        };

        Because of = () => substitutedValue = service.Substitute("something %rostertitle%", new Identity(questionid, new decimal[]{1}), "interviewId");

        It should_replace_roster_title_with_value = () => substitutedValue.ShouldEqual($"something {rosterTitle}");

        static IRosterTitleSubstitutionService service;
        static string rosterTitle;
        static string substitutedValue;
        static Guid questionid;
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), 7);
    }
}

