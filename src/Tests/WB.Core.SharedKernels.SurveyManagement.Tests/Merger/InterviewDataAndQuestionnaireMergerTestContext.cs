using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Merger
{
    internal class InterviewDataAndQuestionnaireMergerTestContext
    {
        internal static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument document)
        {
            return new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(document, 1);
        }

        internal static QuestionnaireRosterStructure CreateQuestionnaireRosterStructureWithOneFixedRoster(Guid fixedRosterId)
        {
            return new QuestionnaireRosterStructure()
            {
                RosterScopes = new Dictionary<Guid, RosterScopeDescription>()
                {
                    {
                        fixedRosterId,
                        new RosterScopeDescription(fixedRosterId, string.Empty, RosterScopeType.Fixed, new Dictionary<Guid, RosterTitleQuestionDescription>()
                        {
                            { fixedRosterId, null }
                        }, new Dictionary<Guid, Guid[]>())
                    }
                }
            };
        }

        internal static void AddInterviewLevel(InterviewData interview, Guid scopeId, decimal[] rosterVector, Dictionary<Guid, object> answeredQuestions, Dictionary<Guid, string> rosterTitles=null)
        {
            InterviewLevel rosterLevel;
            var levelKey = string.Join(",", rosterVector);
            if (!interview.Levels.ContainsKey(levelKey))
            {
                rosterLevel = new InterviewLevel(scopeId, null, rosterVector);
            }
            else
            {
                rosterLevel = interview.Levels[levelKey];
                rosterLevel.ScopeIds.Add(scopeId, null);
            }

            foreach (var answeredQuestion in answeredQuestions)
            {
                var nestedQuestion = rosterLevel.GetOrCreateQuestion(answeredQuestion.Key);
                nestedQuestion.Answer = answeredQuestion.Value;
            }

            if (rosterTitles != null)
            {
                foreach (var rosterTitle in rosterTitles)
                {
                    rosterLevel.RosterRowTitles.Add(rosterTitle.Key, rosterTitle.Value);
                }
            }

            interview.Levels[levelKey] = rosterLevel;
        }

        internal static InterviewQuestionView GetQuestion(InterviewDetailsView interviewDetailsView, Guid questionId,
            decimal[] questionRosterVector)
        {
            var interviewGroupView =
                interviewDetailsView.Groups.FirstOrDefault(
                    g => g.Questions.Any(q => q.Id == questionId) && g.RosterVector.SequenceEqual(questionRosterVector));
            if (
                interviewGroupView != null)
                return interviewGroupView
                    .Questions.FirstOrDefault(q => q.Id == questionId);
            return null;
        }

        internal static ReferenceInfoForLinkedQuestions CreateQuestionnaireReferenceInfo(QuestionnaireDocument questionnaireDocument = null)
        {
            if (questionnaireDocument == null)
                return new ReferenceInfoForLinkedQuestions();
            questionnaireDocument.ConnectChildrenWithParent();
            return new ReferenceInfoForLinkedQuestionsFactory().CreateReferenceInfoForLinkedQuestions(questionnaireDocument, 1);
        }

        internal static QuestionnaireDocument CreateQuestionnaireDocumentWithGroupAndFixedRoster(Guid groupId, string groupTitle, Guid fixedRosterId, string fixedRosterTitle, string[] rosterFixedTitles)
        {
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>
                {
                    new Group(groupTitle)
                    {
                        PublicKey = groupId,
                        IsRoster = false
                    },
                    new Group(fixedRosterTitle)
                    {
                        PublicKey = fixedRosterId,
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                        RosterFixedTitles = rosterFixedTitles
                    }
                }
            };
        }

        internal static QuestionnaireDocumentVersioned CreateQuestionnaireWithVersion(QuestionnaireDocument questionnaireDocument)
        {

            return new QuestionnaireDocumentVersioned
            {
                Version = 1,
                Questionnaire = questionnaireDocument
            };
        }

        internal static InterviewData CreateInterviewDataForQuestionnaireWithGroupAndFixedRoster(Guid interviewId, Guid groupId, string groupTitle, Guid fixedRosterId, string fixedRosterTitle, string[] rosterFixedTitles)
        {
            return new InterviewData()
            {
                InterviewId = interviewId,
                Levels = new Dictionary<string, InterviewLevel>()
                {
                    {
                        "#", 
                        new InterviewLevel(interviewId, null, new decimal[0])
                        {
                            RosterRowTitles = new Dictionary<Guid, string>()
                        }
                    },
                    {
                        "0",
                        new InterviewLevel(fixedRosterId, null, new decimal[] { 0 })
                        {
                            RosterRowTitles = new Dictionary<Guid, string>()
                            {
                                { fixedRosterId, rosterFixedTitles[0] }
                            }
                        }
                    },
                    {
                        "1", 
                        new InterviewLevel(fixedRosterId, null, new decimal[] { 1 })
                        {
                            RosterRowTitles = new Dictionary<Guid, string>()
                            {
                                { fixedRosterId, rosterFixedTitles[1] }
                            }
                        }
                    }
                }
            };
        }

        internal static InterviewData CreateInterviewData(Guid interviewId)
        {
            return new InterviewData()
            {
                InterviewId = interviewId,
                Levels = new Dictionary<string, InterviewLevel>()
                {
                    {
                        "#", 
                        new InterviewLevel(interviewId, null, new decimal[0])
                        {
                            RosterRowTitles = new Dictionary<Guid, string>()
                        }
                    }
                }
            };
        }

        internal static InterviewDataAndQuestionnaireMerger CreateMerger()
        {
            return new InterviewDataAndQuestionnaireMerger();
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