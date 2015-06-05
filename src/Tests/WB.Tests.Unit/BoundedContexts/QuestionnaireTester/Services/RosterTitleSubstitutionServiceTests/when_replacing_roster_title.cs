using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.RosterTitleSubstitutionServiceTests
{
    public class when_replacing_roster_title : RosterTitleSubstitutionServiceTestsContext
    {
        Establish context = () =>
        {
            questionid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitle = "rosterValue";

            var interview = Mock.Of<IStatefulInterview>(x => 
                x.FindRosterByOrDeeperRosterLevel(Moq.It.IsAny<Guid>(), Moq.It.IsAny<decimal[]>()) == 
                    new InterviewRoster { Title = rosterTitle });

            var questionnaire = new QuestionnaireModel { Parents = new Dictionary<Guid, List<QuestionnaireReferenceModel>>() };
            questionnaire.Parents[questionid] = new List<QuestionnaireReferenceModel>
            {
                new QuestionnaireReferenceModel
                {
                    Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"),
                    ModelType = typeof(RosterModel)
                }
            };

            var questionnaireStorageStub = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            questionnaireStorageStub.SetReturnsDefault(questionnaire);

            var interviewRepositoryStub = new Mock<IStatefulInterviewRepository>();
            interviewRepositoryStub.SetReturnsDefault(interview);

            service = new RosterTitleSubstitutionService(questionnaireStorageStub.Object, interviewRepositoryStub.Object, Create.SubstitutionService());
        };

        Because of = () => substitutedValue = service.Substitute("something %rostertitle%", new Identity(questionid, new decimal[]{1}), "interviewId");

        It should_replace_roster_title_with_value = () => substitutedValue.ShouldEqual(string.Format("something {0}", rosterTitle));

        static IRosterTitleSubstitutionService service;
        static string rosterTitle;
        static string substitutedValue;
        static Guid questionid;
    }
}

