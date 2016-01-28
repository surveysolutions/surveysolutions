using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
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
                RosterScopes = new Dictionary<ValueVector<Guid>, RosterScopeDescription>()
                {
                    {
                        new ValueVector<Guid> { fixedRosterId },
                        new RosterScopeDescription(new ValueVector<Guid> { fixedRosterId }, string.Empty, RosterScopeType.Fixed, new Dictionary<Guid, RosterTitleQuestionDescription>()
                        {
                            { fixedRosterId, null }
                        })
                    }
                }
            };
        }

        internal static void AddInterviewLevel(InterviewData interview, ValueVector<Guid> scopeVector,
            decimal[] rosterVector, Dictionary<Guid, object> answeredQuestions,
            Dictionary<Guid, string> rosterTitles = null, int? sortIndex = null)
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

        internal static InterviewDataAndQuestionnaireMerger CreateMerger(QuestionnaireDocument questionnaire, QuestionnaireRosterStructure questionnaireRosters = null)
        {
            var questionnaireReferenceInfo = CreateQuestionnaireReferenceInfo(questionnaire);
            questionnaireRosters = questionnaireRosters ?? CreateQuestionnaireRosterStructure(questionnaire);

            var kvStorageQuestionnaireRosterStructure =
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(
                    x => x.GetById(Moq.It.IsAny<string>()) == questionnaireRosters);

            var kvStorageOfReferenceInfoForLinkedQuestions =
                Mock.Of<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>>(
                    x => x.GetById(Moq.It.IsAny<string>()) == questionnaireReferenceInfo);

            return new InterviewDataAndQuestionnaireMerger(
                substitutionService: new SubstitutionService(),
                questionnaireRosterStructures: kvStorageQuestionnaireRosterStructure,
                questionnaireReferenceInfoForLinkedQuestions: kvStorageOfReferenceInfoForLinkedQuestions);
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