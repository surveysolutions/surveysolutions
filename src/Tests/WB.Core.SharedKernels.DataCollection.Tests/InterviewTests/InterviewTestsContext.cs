﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class InterviewTestsContext
    {
        protected static Interview CreateInterview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            Dictionary<Guid, object> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null)
        {
            return new Interview(
                interviewId ?? new Guid("A0A0A0A0B0B0B0B0A0A0A0A0B0B0B0B0"),
                userId ?? new Guid("F000F000F000F000F000F000F000F000"),
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),1,
                answersToFeaturedQuestions ?? new Dictionary<Guid, object>(),
                answersTime ?? new DateTime(2012, 12, 20),
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"));
        }

        protected static InterviewSynchronizationDto CreateInterviewSynchronizationDto(
            Guid? interviewId = null, InterviewStatus? status = null, Guid? userId = null,
            Guid? questionnaireId = null, long? questionnaireVersion = null,
            AnsweredQuestionSynchronizationDto[] answers = null,
            HashSet<InterviewItemId> disabledGroups = null, HashSet<InterviewItemId> disabledQuestions = null,
            HashSet<InterviewItemId> validAnsweredQuestions = null, HashSet<InterviewItemId> invalidAnsweredQuestions = null,
            Dictionary<InterviewItemId, int> propagatedGroupInstanceCounts = null,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances = null, bool? wasCompleted = false)
        {
            return new InterviewSynchronizationDto(
                interviewId ?? new Guid("A1A1A1A1B1B1B1B1A1A1A1A1B1B1B1B1"),
                status ?? InterviewStatus.RejectedBySupervisor,
                userId ?? new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId ?? new Guid("B111B111B111B111B111B111B111B111"),
                questionnaireVersion ?? 1,
                answers ?? new AnsweredQuestionSynchronizationDto[]{},
                disabledGroups ?? new HashSet<InterviewItemId>(),
                disabledQuestions ?? new HashSet<InterviewItemId>(),
                validAnsweredQuestions ?? new HashSet<InterviewItemId>(),
                invalidAnsweredQuestions ?? new HashSet<InterviewItemId>(),
                propagatedGroupInstanceCounts ?? new Dictionary<InterviewItemId, int>(),
                rosterGroupInstances ?? new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(),
                wasCompleted ?? false);
        }

        protected static IQuestionnaireRepository CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire)
        {
            return Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);
        }

        protected static void SetupInstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }
    }
}