using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.CascadingComboboxTests
{
    [Subject(typeof(InterviewViewModel))]
    internal class InterviewViewModelTestContext
    {
        protected static Guid cascadingC1Id = Guid.Parse("11111111111111111111111111111111");
        protected static Guid cascadingC2Id = Guid.Parse("22222222222222222222222222222222");
        protected static Guid cascadingC3Id = Guid.Parse("33333333333333333333333333333333");
        protected static Guid cascadingC4Id = Guid.Parse("44444444444444444444444444444444");
        protected static Guid cascadingC5Id = Guid.Parse("55555555555555555555555555555555");
        protected static Guid cascadingC6Id = Guid.Parse("66666666666666666666666666666666");
        protected static Guid firstLevelRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        protected static Guid secondLevelRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        protected static QuestionnaireDocument CreateQuestionnaireWithCascadingQuestions()
        {
            return CreateQuestionnaireDocumentWithOneChapter(
                new SingleQuestion
                {
                    QuestionType = QuestionType.SingleOption,
                    PublicKey = cascadingC1Id,
                    StataExportCaption = "c1",
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "c1 Option 1", AnswerValue = "1" },
                        new Answer { AnswerText = "c1 Option 2", AnswerValue = "2" }
                    },
                },
                new SingleQuestion
                {
                    QuestionType = QuestionType.SingleOption,
                    PublicKey = cascadingC2Id,
                    CascadeFromQuestionId = cascadingC1Id,
                    StataExportCaption = "c2",
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "c2 Option 1.1", AnswerValue = "1", ParentValue = "1" },
                        new Answer { AnswerText = "c2 Option 1.2", AnswerValue = "2", ParentValue = "1" },
                        new Answer { AnswerText = "c2 Option 2.1", AnswerValue = "3", ParentValue = "2" },
                        new Answer { AnswerText = "c2 Option 2.2", AnswerValue = "4", ParentValue = "2" }
                    },
                },
                new Group
                {
                    PublicKey = Guid.NewGuid(),
                    Children = new List<IComposite>
                    {
                        new SingleQuestion
                        {
                            QuestionType = QuestionType.SingleOption,
                            PublicKey = cascadingC3Id,
                            CascadeFromQuestionId = cascadingC2Id,
                            StataExportCaption = "c3",
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "c3 Option 1.1.1", AnswerValue = "1", ParentValue = "1" },
                                new Answer { AnswerText = "c3 Option 1.1.2", AnswerValue = "2", ParentValue = "1" },
                                new Answer { AnswerText = "c3 Option 1.1.3", AnswerValue = "3", ParentValue = "1" },
                                new Answer { AnswerText = "c3 Option 1.1.4", AnswerValue = "4", ParentValue = "1" },

                                new Answer { AnswerText = "c3 Option 1.2.1", AnswerValue = "5", ParentValue = "2" },
                                new Answer { AnswerText = "c3 Option 1.2.2", AnswerValue = "6", ParentValue = "2" },
                                new Answer { AnswerText = "c3 Option 1.2.3", AnswerValue = "7", ParentValue = "2" },
                                new Answer { AnswerText = "c3 Option 1.2.4", AnswerValue = "8", ParentValue = "2" },

                                new Answer { AnswerText = "c3 Option 2.1.1", AnswerValue = "9", ParentValue = "3" },
                                new Answer { AnswerText = "c3 Option 2.1.2", AnswerValue = "10", ParentValue = "3" },
                                new Answer { AnswerText = "c3 Option 2.1.3", AnswerValue = "11", ParentValue = "3" },
                                new Answer { AnswerText = "c3 Option 2.1.4", AnswerValue = "12", ParentValue = "3" },

                                new Answer { AnswerText = "c3 Option 2.2.1", AnswerValue = "13", ParentValue = "4" },
                                new Answer { AnswerText = "c3 Option 2.2.2", AnswerValue = "14", ParentValue = "4" },
                                new Answer { AnswerText = "c3 Option 2.2.3", AnswerValue = "15", ParentValue = "4" },
                                new Answer { AnswerText = "c3 Option 2.2.4", AnswerValue = "16", ParentValue = "4" }
                            },
                        }
                    }
                },
                new Group
                {
                    PublicKey = firstLevelRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "r 1", "r 2" },
                    Children = new List<IComposite>
                    {
                        new SingleQuestion()
                        {
                            PublicKey = cascadingC4Id,
                            StataExportCaption = "c4",
                            CascadeFromQuestionId = cascadingC1Id,
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "c4 Option 1.1", AnswerValue = "1", ParentValue = "1" },
                                new Answer { AnswerText = "c4 Option 1.2", AnswerValue = "2", ParentValue = "1" },
                                new Answer { AnswerText = "c4 Option 2.1", AnswerValue = "3", ParentValue = "2" },
                                new Answer { AnswerText = "c4 Option 2.2", AnswerValue = "4", ParentValue = "2" }
                            },
                        },
                        new Group
                        {
                            PublicKey = secondLevelRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.FixedTitles,
                            RosterFixedTitles = new[] { "r 1.1", "r 1.2" },
                            Children = new List<IComposite>
                            {
                                new SingleQuestion
                                {
                                    PublicKey = cascadingC5Id,
                                    StataExportCaption = "c5",
                                    CascadeFromQuestionId = cascadingC4Id,
                                    Answers = new List<Answer>
                                    {
                                        new Answer { AnswerText = "c5 Option 1.1.1", AnswerValue = "1", ParentValue = "1" },
                                        new Answer { AnswerText = "c5 Option 1.1.2", AnswerValue = "2", ParentValue = "1" },
                                        new Answer { AnswerText = "c5 Option 1.1.3", AnswerValue = "3", ParentValue = "1" },
                                        new Answer { AnswerText = "c5 Option 1.1.4", AnswerValue = "4", ParentValue = "1" },

                                        new Answer { AnswerText = "c5 Option 1.2.1", AnswerValue = "5", ParentValue = "2" },
                                        new Answer { AnswerText = "c5 Option 1.2.2", AnswerValue = "6", ParentValue = "2" },
                                        new Answer { AnswerText = "c5 Option 1.2.3", AnswerValue = "7", ParentValue = "2" },
                                        new Answer { AnswerText = "c5 Option 1.2.4", AnswerValue = "8", ParentValue = "2" },

                                        new Answer { AnswerText = "c5 Option 2.1.1", AnswerValue = "9", ParentValue = "3" },
                                        new Answer { AnswerText = "c5 Option 2.1.2", AnswerValue = "10", ParentValue = "3" },
                                        new Answer { AnswerText = "c5 Option 2.1.3", AnswerValue = "11", ParentValue = "3" },
                                        new Answer { AnswerText = "c5 Option 2.1.4", AnswerValue = "12", ParentValue = "3" },

                                        new Answer { AnswerText = "c5 Option 2.2.1", AnswerValue = "13", ParentValue = "4" },
                                        new Answer { AnswerText = "c5 Option 2.2.2", AnswerValue = "14", ParentValue = "4" },
                                        new Answer { AnswerText = "c5 Option 2.2.3", AnswerValue = "15", ParentValue = "4" },
                                        new Answer { AnswerText = "c5 Option 2.2.4", AnswerValue = "16", ParentValue = "4" }
                                    },
                                },
                                new SingleQuestion
                                {
                                    PublicKey = cascadingC6Id,
                                    StataExportCaption = "c6",
                                    CascadeFromQuestionId = cascadingC2Id,
                                    Answers = new List<Answer>
                                    {
                                        new Answer { AnswerText = "c6 Option 1.1.1", AnswerValue = "1", ParentValue = "1" },
                                        new Answer { AnswerText = "c6 Option 1.1.2", AnswerValue = "2", ParentValue = "1" },
                                        new Answer { AnswerText = "c6 Option 1.1.3", AnswerValue = "3", ParentValue = "1" },
                                        new Answer { AnswerText = "c6 Option 1.1.4", AnswerValue = "4", ParentValue = "1" },

                                        new Answer { AnswerText = "c6 Option 1.2.1", AnswerValue = "5", ParentValue = "2" },
                                        new Answer { AnswerText = "c6 Option 1.2.2", AnswerValue = "6", ParentValue = "2" },
                                        new Answer { AnswerText = "c6 Option 1.2.3", AnswerValue = "7", ParentValue = "2" },
                                        new Answer { AnswerText = "c6 Option 1.2.4", AnswerValue = "8", ParentValue = "2" },

                                        new Answer { AnswerText = "c6 Option 2.1.1", AnswerValue = "9", ParentValue = "3" },
                                        new Answer { AnswerText = "c6 Option 2.1.2", AnswerValue = "10", ParentValue = "3" },
                                        new Answer { AnswerText = "c6 Option 2.1.3", AnswerValue = "11", ParentValue = "3" },
                                        new Answer { AnswerText = "c6 Option 2.1.4", AnswerValue = "12", ParentValue = "3" },

                                        new Answer { AnswerText = "c6 Option 2.2.1", AnswerValue = "13", ParentValue = "4" },
                                        new Answer { AnswerText = "c6 Option 2.2.2", AnswerValue = "14", ParentValue = "4" },
                                        new Answer { AnswerText = "c6 Option 2.2.3", AnswerValue = "15", ParentValue = "4" },
                                        new Answer { AnswerText = "c6 Option 2.2.4", AnswerValue = "16", ParentValue = "4" }
                                    },
                                }

                            }
                        }
                    }
                });
        }

        protected static InterviewViewModel CreateInterviewViewModel(QuestionnaireDocument template,
            QuestionnaireRosterStructure rosterStructure, InterviewSynchronizationDto interviewSynchronizationDto)
        {
            var result = new InterviewViewModel(Guid.NewGuid(), template, rosterStructure,                 interviewSynchronizationDto);

            foreach (var screen in result.Chapters)
            {
                SubscribeScreen(result, screen);
            }

            return result;
        }

        protected static InterviewViewModel CreateInterviewViewModel(QuestionnaireDocument template, QuestionnaireRosterStructure rosterStructure)
        {
            var result = new InterviewViewModel(Guid.NewGuid(), template, rosterStructure);

            foreach (var screen in result.Chapters)
            {
                SubscribeScreen(result, screen);
            }

            foreach (var featuredQuestion in result.FeaturedQuestions)
            {
                SubscribeObject(featuredQuestion.Value);
            }
            return result;
        }

        protected static void PropagateScreen(InterviewViewModel interviewViewModel, Guid screenId, decimal rosterInstanceId, decimal[] outerScopePropagationVector = null)
        {
            var outerVector = outerScopePropagationVector ?? new decimal[0];
            interviewViewModel.AddRosterScreen(screenId, outerVector, rosterInstanceId, null);

            var extendedVector = outerVector.ToList();
            extendedVector.Add(rosterInstanceId);
            var newScreen = interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(screenId, extendedVector.ToArray())] as QuestionnaireScreenViewModel;
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
                        SubscribeScreen(interviewViewModel, interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(questionnaireNavigationPanelItem.PublicKey.Id, questionnaireNavigationPanelItem.PublicKey.InterviewItemPropagationVector)]);
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
            return new InterviewSynchronizationDto(id: Guid.NewGuid(), status: InterviewStatus.InterviewerAssigned, comments: null,
                userId: Guid.NewGuid(), questionnaireId: Guid.NewGuid(), questionnaireVersion: 1,
                answers: answers,
                disabledGroups: new HashSet<InterviewItemId>(),
                disabledQuestions: new HashSet<InterviewItemId>(), validAnsweredQuestions: new HashSet<InterviewItemId>(),
                invalidAnsweredQuestions: new HashSet<InterviewItemId>(),
                propagatedGroupInstanceCounts: null,
                rosterGroupInstances: propagatedGroupInstanceCounts,
                wasCompleted: false);
        }

        protected static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument questionnaire)
        {
            return new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(questionnaire, 1);
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
