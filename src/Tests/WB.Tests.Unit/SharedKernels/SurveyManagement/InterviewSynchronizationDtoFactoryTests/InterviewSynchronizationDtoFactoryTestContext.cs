﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    internal class InterviewSynchronizationDtoFactoryTestContext
    {
        protected static InterviewSynchronizationDtoFactory CreateInterviewSynchronizationDtoFactory(QuestionnaireDocument document)
        {
            document.ConnectChildrenWithParent();
            return new InterviewSynchronizationDtoFactory(
                Mock.Of<IReadSideRepositoryWriter<InterviewStatuses>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireRosterStructure>>(
                    _ =>
                        _.GetById(Moq.It.IsAny<string>()) ==
                        new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(document, 1)), 
                Mock.Of<IReadSideKeyValueStorage<InterviewLinkedQuestionOptions>>(),
                Mock.Of<IQuestionnaireStorage>(_=>_.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())== document));
        }

        protected static InterviewSynchronizationDtoFactory CreateInterviewSynchronizationDtoFactory(QuestionnaireRosterStructure questionnaireRosterStructure)
        {
            return new InterviewSynchronizationDtoFactory(
                Mock.Of<IReadSideRepositoryWriter<InterviewStatuses>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireRosterStructure>>(
                    _ =>
                        _.GetById(Moq.It.IsAny<string>()) ==
                        questionnaireRosterStructure),
                Mock.Of<IReadSideKeyValueStorage<InterviewLinkedQuestionOptions>>(),
                Mock.Of<IQuestionnaireStorage>());
        }

        internal static InterviewData CreateInterviewData(Guid? interviewId=null)
        {
            return new InterviewData()
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
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

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            var result = new QuestionnaireDocument
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

        internal static void AddInterviewLevel(
            InterviewData interview, 
            ValueVector<Guid> scopeVector, 
            decimal[] rosterVector, 
            Dictionary<Guid, object> answeredQuestions = null, 
            Dictionary<Guid, string> rosterTitles = null,
            Dictionary<Guid, object> variables = null,
            HashSet<Guid> disableVariables=null)
        {
            InterviewLevel rosterLevel;
            var levelKey = string.Join(",", rosterVector);
            if (!interview.Levels.ContainsKey(levelKey))
            {
                rosterLevel = new InterviewLevel(scopeVector, null, rosterVector);
            }
            else
            {
                rosterLevel = interview.Levels[levelKey];
                rosterLevel.ScopeVectors.Add(scopeVector, null);
            }
            if (answeredQuestions != null)
            {
                foreach (var answeredQuestion in answeredQuestions)
                {
                    if (!rosterLevel.QuestionsSearchCache.ContainsKey(answeredQuestion.Key))
                        rosterLevel.QuestionsSearchCache.Add(answeredQuestion.Key,
                            new InterviewQuestion(answeredQuestion.Key));

                    var nestedQuestion = rosterLevel.QuestionsSearchCache[answeredQuestion.Key];

                    nestedQuestion.Answer = answeredQuestion.Value;
                }
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
            if (disableVariables != null)
            {
                rosterLevel.DisabledVariables = disableVariables;
            }
            interview.Levels[levelKey] = rosterLevel;
        }
    }
}
