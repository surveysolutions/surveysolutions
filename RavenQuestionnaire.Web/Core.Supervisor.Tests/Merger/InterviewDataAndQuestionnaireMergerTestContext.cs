using System;
using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.Views;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Tests.Merger
{
    internal class InterviewDataAndQuestionnaireMergerTestContext
    {
        internal static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument document)
        {
            return new QuestionnaireRosterStructure(document, 1);
        }

        internal static QuestionnaireRosterStructure CreateQuestionnaireRosterStructureWithOneFixedRoster(Guid fixedRosterId)
        {
            return new QuestionnaireRosterStructure()
            {
                RosterScopes = new Dictionary<Guid, RosterScopeDescription>()
                {
                    {
                        fixedRosterId,
                        new RosterScopeDescription(fixedRosterId, new Dictionary<Guid, RosterTitleQuestionDescription>()
                        {
                            { fixedRosterId, null }
                        })
                    }
                }
            };
        }

        internal static void AddInterviewLevel(InterviewData interview, Guid scopeId, decimal[] rosterVector, Dictionary<Guid,object> answeredQuestions)
        {
            var rosterLevel = new InterviewLevel(scopeId, null, rosterVector);
            foreach (var answeredQuestion in answeredQuestions)
            {
                var nestedQuestion = rosterLevel.GetOrCreateQuestion(answeredQuestion.Key);
                nestedQuestion.Answer = answeredQuestion.Value;
            }
            interview.Levels.Add(string.Join(",", rosterVector), rosterLevel);
        }

        internal static ReferenceInfoForLinkedQuestions CreateQuestionnaireReferenceInfo()
        {
            return new ReferenceInfoForLinkedQuestions();
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