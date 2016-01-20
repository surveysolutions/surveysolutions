using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class InterviewTestsContext
    {
        protected static Interview CreateInterview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            Dictionary<Guid, object> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null,
            IPlainQuestionnaireRepository questionnaireRepository = null, 
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = Create.Interview(
                questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: expressionProcessorStatePrototypeProvider);

            interview.CreateInterview(
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),
                1,
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"),
                answersToFeaturedQuestions ?? new Dictionary<Guid, object>(),
                answersTime ?? new DateTime(2012, 12, 20),
                userId ?? new Guid("F000F000F000F000F000F000F000F000"));

            return interview;
        }

        protected static InterviewSynchronizationDto CreateInterviewSynchronizationDto(
            Guid? interviewId = null, InterviewStatus? status = null, Guid? userId = null,
            Guid? questionnaireId = null, long? questionnaireVersion = null,
            AnsweredQuestionSynchronizationDto[] answers = null,
            HashSet<InterviewItemId> disabledGroups = null, HashSet<InterviewItemId> disabledQuestions = null,
            HashSet<InterviewItemId> validAnsweredQuestions = null, HashSet<InterviewItemId> invalidAnsweredQuestions = null,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances = null, bool? wasCompleted = false)
        {
            return new InterviewSynchronizationDto(
                interviewId ?? new Guid("A1A1A1A1B1B1B1B1A1A1A1A1B1B1B1B1"),
                status ?? InterviewStatus.RejectedBySupervisor, 
                null, 
                null,
                null,
                userId ?? new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId ?? new Guid("B111B111B111B111B111B111B111B111"),
                questionnaireVersion ?? 1,
                answers ?? new AnsweredQuestionSynchronizationDto[]{},
                disabledGroups ?? new HashSet<InterviewItemId>(),
                disabledQuestions ?? new HashSet<InterviewItemId>(),
                validAnsweredQuestions ?? new HashSet<InterviewItemId>(),
                invalidAnsweredQuestions ?? new HashSet<InterviewItemId>(),
                rosterGroupInstances ?? new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(),
                wasCompleted ?? false);
        }

        protected static IPlainQuestionnaireRepository CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire = null)
        {
            return Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument();
            var chapter = new Group("Chapter");
            result.Children.Add(chapter);

            foreach (var child in children)
            {
                chapter.Children.Add(child);
            }

            return result;
        }
    }
}