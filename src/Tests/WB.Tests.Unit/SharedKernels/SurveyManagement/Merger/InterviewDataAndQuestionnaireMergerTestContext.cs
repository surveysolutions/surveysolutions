using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class InterviewDataAndQuestionnaireMergerTestContext
    {
        
        internal static void AddInterviewLevel(InterviewData interview, ValueVector<Guid> scopeVector,
            decimal[] rosterVector, Dictionary<Guid, object> answeredQuestions = null,
            Dictionary<Guid, string> rosterTitles = null, int? sortIndex = null, Dictionary<Guid, object> variables = null)
        {
            InterviewLevel rosterLevel;
            var levelKey = string.Join(",", rosterVector);
            if (!interview.Levels.ContainsKey(levelKey))
            {
                rosterLevel = new InterviewLevel(scopeVector, sortIndex, rosterVector);
            }
            else
            {
                rosterLevel = interview.Levels[levelKey];
                rosterLevel.ScopeVectors.Add(scopeVector, sortIndex);
            }
            if (answeredQuestions != null)
                foreach (var answeredQuestion in answeredQuestions)
                {
                    if (!rosterLevel.QuestionsSearchCache.ContainsKey(answeredQuestion.Key))
                        rosterLevel.QuestionsSearchCache.Add(answeredQuestion.Key,
                            new InterviewQuestion(answeredQuestion.Key));

                    var nestedQuestion = rosterLevel.QuestionsSearchCache[answeredQuestion.Key];
                    nestedQuestion.Answer = answeredQuestion.Value;
                }

            if (rosterTitles != null)
            {
                foreach (var rosterTitle in rosterTitles)
                {
                    rosterLevel.RosterRowTitles.Add(rosterTitle.Key, rosterTitle.Value);
                }
            }
            if (variables != null)
            {
                rosterLevel.Variables = variables;
            }
            interview.Levels[levelKey] = rosterLevel;
        }

        internal static InterviewQuestionView GetQuestion(InterviewDetailsView interviewDetailsView, Guid questionId,
            decimal[] questionRosterVector)
        {
            var interviewGroupView =
                interviewDetailsView.Groups.FirstOrDefault(
                    g => g.Entities.Any(q => q.Id == questionId) && g.RosterVector.SequenceEqual(questionRosterVector));
            if (
                interviewGroupView != null)
                return interviewGroupView
                    .Entities.OfType<InterviewQuestionView>().FirstOrDefault(q => q.Id == questionId);
            return null;
        }

        internal static InterviewStaticTextView GetStaticText(InterviewDetailsView interviewDetailsView, 
            Guid staticTextId,
            decimal[] questionRosterVector)
        {
            var interviewGroupView = interviewDetailsView.Groups.FirstOrDefault(g => 
                g.Entities.Any(q => q.Id == staticTextId) && 
                g.RosterVector.SequenceEqual(questionRosterVector));

            return interviewGroupView?.Entities.OfType<InterviewStaticTextView>().FirstOrDefault(q => q.Id == staticTextId);
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
                    Create.Entity.FixedRoster(rosterId: fixedRosterId, fixedTitles: rosterFixedTitles, title:fixedRosterTitle)
                }
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
                        new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0])
                        {
                            RosterRowTitles = new Dictionary<Guid, string>()
                        }
                    },
                    {
                        "0",
                        new InterviewLevel(new ValueVector<Guid>{fixedRosterId}, null, new decimal[] { 0 })
                        {
                            RosterRowTitles = new Dictionary<Guid, string>()
                            {
                                { fixedRosterId, rosterFixedTitles[0] }
                            }
                        }
                    },
                    {
                        "1", 
                        new InterviewLevel(new ValueVector<Guid>{fixedRosterId}, null, new decimal[] { 1 })
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
                        new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0])
                        {
                            RosterRowTitles = new Dictionary<Guid, string>()
                        }
                    }
                }
            };
        }

        internal static InterviewDataAndQuestionnaireMerger CreateMerger(QuestionnaireDocument questionnaire)
        {
            var substitutionService = new SubstitutionService();
            return new InterviewDataAndQuestionnaireMerger(
                substitutionService: substitutionService,
                variableToUiStringService: new VariableToUIStringService(),
                interviewEntityViewFactory: new InterviewEntityViewFactory(substitutionService));
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
           var result =  new QuestionnaireDocument
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
            result.ConnectChildrenWithParent();
            return result;
        }
    }
}