using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class InterviewTestsContext
    {
        protected static Interview CreateInterview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null,
            IQuestionnaireStorage questionnaireRepository = null, 
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var textFactory = Create.Service.SubstitionTextFactory();
            var interview = Create.AggregateRoot.Interview(
                questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: expressionProcessorStatePrototypeProvider,
                textFactory: textFactory);

            interview.CreateInterview(
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),
                1,
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"),
                answersToFeaturedQuestions ?? new Dictionary<Guid, AbstractAnswer>(),
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
            return Create.Entity.InterviewSynchronizationDto(
                interviewId: interviewId ?? new Guid("A1A1A1A1B1B1B1B1A1A1A1A1B1B1B1B1"),
                status: status ?? InterviewStatus.RejectedBySupervisor,
                userId: userId ?? new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId: questionnaireId ?? new Guid("B111B111B111B111B111B111B111B111"),
                questionnaireVersion: questionnaireVersion ?? 1,
                wasCompleted: wasCompleted ?? false
                );
        }

        protected static IQuestionnaireStorage CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire = null)
        {
            return Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionaire);
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