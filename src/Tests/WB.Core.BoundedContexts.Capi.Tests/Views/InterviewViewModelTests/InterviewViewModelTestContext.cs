﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Supervisor.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    [Subject(typeof(InterviewViewModel))]
    internal class InterviewViewModelTestContext
    {
        protected static InterviewViewModel CreateInterviewViewModel(QuestionnaireDocument template,
            QuestionnaireRosterStructure rosterStructure, InterviewSynchronizationDto interviewSynchronizationDto)
        {
            var result = new InterviewViewModel(Guid.NewGuid(), template, rosterStructure,
                interviewSynchronizationDto);

            foreach (var screen in result.Chapters)
            {
                SubscribeScreen(result, screen);
            }
            
            return result;
        }

        protected static void PropagateScreen(InterviewViewModel interviewViewModel, Guid screenId, decimal rosterInstanceId, decimal[] outerScopePropagationVector=null)
        {
            var outerVector = outerScopePropagationVector ?? new decimal[0];
            interviewViewModel.AddPropagateScreen(screenId, outerVector, rosterInstanceId, null);

            var extendedVector = outerVector.ToList();
            extendedVector.Add(rosterInstanceId);
            var newScreen = interviewViewModel.Screens[new InterviewItemId(screenId, extendedVector.ToArray())] as QuestionnaireScreenViewModel;
            SubscribeScreen(interviewViewModel, newScreen);
        }

        private static void SubscribeScreen(InterviewViewModel interviewViewModel, IQuestionnaireViewModel screen)
        {
            SubscribeObject(screen);

            var questionnaireScreenViewModel = screen as QuestionnaireScreenViewModel;
            if (questionnaireScreenViewModel != null)
            {
                foreach (var item in questionnaireScreenViewModel.Items)
                {
                    SubscribeObject(item);

                    var questionnaireNavigationPanelItem = item as QuestionnaireNavigationPanelItem;
                    if (questionnaireNavigationPanelItem != null)
                        SubscribeScreen(interviewViewModel, interviewViewModel.Screens[questionnaireNavigationPanelItem.PublicKey]);
                }

                var questionnairePropagatedScreenViewModel = questionnaireScreenViewModel as QuestionnairePropagatedScreenViewModel;
                if (questionnairePropagatedScreenViewModel != null)
                {
                    SubscribeObject(questionnairePropagatedScreenViewModel.Next);
                    SubscribeObject(questionnairePropagatedScreenViewModel.Previous);   
                }
            }
            var questionnaireGridViewModel = screen as QuestionnaireGridViewModel;
            if (questionnaireGridViewModel != null)
            {
                foreach (var questionnairePropagatedScreenViewModel in questionnaireGridViewModel.Rows)
                {
                    SubscribeScreen(interviewViewModel, questionnairePropagatedScreenViewModel);
                }
            }
        }

        private static void SubscribeObject(object o)
        {
            var mvxNotifyScreenPropertyChanged = o as MvxNotifyPropertyChanged;
            if (mvxNotifyScreenPropertyChanged != null)
            {
                mvxNotifyScreenPropertyChanged.ShouldAlwaysRaiseInpcOnUserInterfaceThread(false);
            }
        }

        protected static InterviewSynchronizationDto CreateInterviewSynchronizationDto(AnsweredQuestionSynchronizationDto[] answers,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> propagatedGroupInstanceCounts)
        {
            return new InterviewSynchronizationDto(id: Guid.NewGuid(), status: InterviewStatus.InterviewerAssigned,
                userId: Guid.NewGuid(), questionnaireId: Guid.NewGuid(), questionnaireVersion: 1,
                answers: answers,
                disabledGroups: new HashSet<InterviewItemId>(),
                disabledQuestions: new HashSet<InterviewItemId>(), validAnsweredQuestions: new HashSet<InterviewItemId>(),
                invalidAnsweredQuestions: new HashSet<InterviewItemId>(),
                propagatedGroupInstanceCounts:null,
                rosterGroupInstances: propagatedGroupInstanceCounts,
                wasCompleted: false);
        }

        protected static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument template)
        {
            return new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(template, 1);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
        }
    }
}
