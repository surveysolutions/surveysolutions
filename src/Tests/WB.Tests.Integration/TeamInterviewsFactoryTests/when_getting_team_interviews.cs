using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.TeamInterviewsFactoryTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.TeamInterviewsFactoryTests
{
    internal class when_getting_team_interviews : TeamInterviewsFactoryTestContext
    {
        Establish context = () =>
        {
            Guid responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            version = 1;

            List<InterviewSummary> interviews = new List<InterviewSummary>
            {
                Create.InterviewSummary(status: InterviewStatus.ApprovedBySupervisor, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Create.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Create.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: 2),
                Create.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: Guid.NewGuid(), questionnaireVersion: version),
                Create.InterviewSummary(status: InterviewStatus.ApprovedByHeadquarters, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Create.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Create.InterviewSummary(status: InterviewStatus.InterviewerAssigned, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
            };

            PostgreReadSideStorage<InterviewSummary> repository;
            PostgreReadSideStorage<QuestionAnswer> featuredQuestionAnswersReader;
            reportFactory = CreateTeamInterviewsFactory(out repository, out featuredQuestionAnswersReader);

            ExecuteInCommandTransaction(() => interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid())));

        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.Load(new TeamInterviewsInputModel()
        {
            QuestionnaireId = questionnaireId,
            QuestionnaireVersion = version,
        }));

        It should_return_correct_total_count = () => report.TotalCount.ShouldEqual(3);

        static ITeamInterviewsFactory reportFactory;
        static TeamInterviewsView report;
        static Guid questionnaireId;
        static int version;
    }
}