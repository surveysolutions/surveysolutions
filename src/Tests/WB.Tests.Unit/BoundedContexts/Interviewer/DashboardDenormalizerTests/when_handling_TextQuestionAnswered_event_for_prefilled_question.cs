﻿using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    public class when_handling_TextQuestionAnswered_event_for_prefilled_question
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("22222222222222222222222222222222");

            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("33333333333333333333333333333333"), 1);
            var questionId = Guid.Parse("11111111111111111111111111111111");

            @event = Create.Event.TextQuestionAnswered(questionId, answer: "answer").ToPublishedEvent(interviewId);

            interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewViewStorage.Store(Create.Entity.InterviewView(interviewId: interviewId,
                questionnaireId: questionnaireIdentity.ToString()));

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaire: Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId, new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: questionId, preFilled: true)
                    })));

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_interview_be_new_for_interviewer = () =>
            interviewViewStorage.GetById(interviewId.FormatGuid())?.StartedDateTime.ShouldBeNull();

        private static InterviewerDashboardEventHandler denormalizer;
        private static IPublishedEvent<TextQuestionAnswered> @event;
        private static Guid interviewId;
        private static SqliteInmemoryStorage<InterviewView> interviewViewStorage;
    }
}