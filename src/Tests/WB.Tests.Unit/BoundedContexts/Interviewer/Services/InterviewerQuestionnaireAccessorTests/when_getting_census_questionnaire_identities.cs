using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class when_getting_census_questionnaire_identities : InterviewerQuestionnaireAccessorTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaireAsyncPlainStorage = new SqliteInmemoryStorage<QuestionnaireView>();
            questionnaireAsyncPlainStorage.Store(emulatedStorageQuestionnaires);
            interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(questionnaireViewRepository: questionnaireAsyncPlainStorage);

            BecauseOf();
        }

        public void BecauseOf() =>
             resultCensusQuestionnairesIds = interviewerQuestionnaireAccessor.GetCensusQuestionnaireIdentities();

        [Test]
        public void should_result_contains_only_census_questionnaire_identities() =>
            resultCensusQuestionnairesIds.All(questionnaireIdentity =>
                questionnaireIdentity.ToString() == firstCensusQuestionnaireIdentity.ToString() ||
                questionnaireIdentity.ToString() == secondCensusQuestionnaireIdentity.ToString()).Should().BeTrue();

        static readonly string firstCensusQuestionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1).ToString();
        static readonly string secondCensusQuestionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("22222222222222222222222222222222"), 2).ToString();
        static readonly string nonCensusQuestionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("33333333333333333333333333333333"), 3).ToString();
        static readonly List<QuestionnaireView> emulatedStorageQuestionnaires = new List<QuestionnaireView>()
        {
            new QuestionnaireView { Id = firstCensusQuestionnaireIdentity, Census = true},
            new QuestionnaireView { Id  = secondCensusQuestionnaireIdentity, Census = true},
            new QuestionnaireView { Id = nonCensusQuestionnaireIdentity, Census = false}
        };

        static List<QuestionnaireIdentity> resultCensusQuestionnairesIds;
        static InterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
    }
}
