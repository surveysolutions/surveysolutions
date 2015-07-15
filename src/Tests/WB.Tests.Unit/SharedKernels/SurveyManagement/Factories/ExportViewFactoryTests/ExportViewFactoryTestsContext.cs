using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    [Subject(typeof(ExportViewFactory))]
    internal class ExportViewFactoryTestsContext
    {
        protected static ExportViewFactory CreateExportViewFactory(
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory = null)
        {
            return new ExportViewFactory(new ReferenceInfoForLinkedQuestionsFactory(), questionnaireRosterStructureFactory ?? new QuestionnaireRosterStructureFactory(), Mock.Of<IFileSystemAccessor>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            var questionnaireDocument= new QuestionnaireDocument
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
            questionnaireDocument.ConnectChildrenWithParent();
            return questionnaireDocument;
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Dictionary<string, Guid> variableNameAndQuestionId)
        {
            var questionnaire = new QuestionnaireDocument();

            foreach (var question in variableNameAndQuestionId)
            {
                questionnaire.Children.Add(new NumericQuestion() { StataExportCaption = question.Key, PublicKey = question.Value, QuestionType = QuestionType.Numeric });
            }

            return questionnaire;
        }

        protected static InterviewDataExportLevelView GetLevel(InterviewDataExportView interviewDataExportView, Guid[] levelVector)
        {
            return interviewDataExportView.Levels.FirstOrDefault(l => l.LevelVector.SequenceEqual(levelVector));
        }

        protected static InterviewData CreateInterviewData()
        {
            var interviewData = new InterviewData() { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0]));
            return interviewData;
        }

        protected static InterviewData CreateInterviewWithAnswers(IEnumerable<Guid> questionsWithAnswers)
        {
            var interviewData = CreateInterviewData();
            foreach (var questionsWithAnswer in questionsWithAnswers)
            {
                if (!interviewData.Levels["#"].QuestionsSearchCahche.ContainsKey(questionsWithAnswer))
                    interviewData.Levels["#"].QuestionsSearchCahche.Add(questionsWithAnswer, new InterviewQuestion(questionsWithAnswer));

                var question = interviewData.Levels["#"].QuestionsSearchCahche[questionsWithAnswer]; 
                
                question.Answer = "some answer";
            }
            return interviewData;
        }
    }
}
