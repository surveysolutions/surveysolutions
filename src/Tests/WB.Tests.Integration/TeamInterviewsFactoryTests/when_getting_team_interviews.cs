using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
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
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.ApprovedBySupervisor, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: 2),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: Guid.NewGuid(), questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.ApprovedByHeadquarters, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.InterviewerAssigned, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
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