using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewsToDeleteFactoryTests
{
    [Subject(typeof(InterviewsToDeleteFactory))]
    internal class when_questionnaireId_and_version_provided 
    {
        Establish context = () =>
        {
            var interviews = new List<InterviewSummary>();
            questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaireVersion = 1;
            expectedSummary = Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion);
            interviews.Add(Create.Entity.InterviewSummary(questionnaireId: Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), questionnaireVersion: questionnaireVersion));
            interviews.Add(expectedSummary);
            interviews.Add(Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: 2));

            TestInMemoryWriter<InterviewSummary> writer = Stub.ReadSideRepository<InterviewSummary>();
            interviews.ForEach(x => writer.Store(x, Guid.NewGuid().FormatGuid()));


            factory = new InterviewsToDeleteFactory(writer);
        };

        Because of = () => foundSummaries = factory.Load(questionnaireId, questionnaireVersion);

        It should_return_interviews_by_version_and_questionnaire_id = () =>
        {
            foundSummaries.Count.ShouldEqual(1);
            foundSummaries.First().ShouldBeLike(expectedSummary);
        };

        static InterviewsToDeleteFactory factory;
        static Guid questionnaireId;
        static int questionnaireVersion;
        static List<InterviewSummary> foundSummaries;
        private static InterviewSummary expectedSummary;
    }
}

