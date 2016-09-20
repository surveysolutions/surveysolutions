using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    [Subject(typeof(ExportViewFactory))]
    internal class ExportViewFactoryTestsContext
    {
        protected static ExportViewFactory CreateExportViewFactory(
            IQuestionnaireStorage questionnaireStorage = null,
            IExportQuestionService exportQuestionService = null)
        {
            return new ExportViewFactory(
                Mock.Of<IFileSystemAccessor>(),
                exportQuestionService ?? new ExportQuestionService(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter( params IComposite[] chapterChildren)
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
                if (!interviewData.Levels["#"].QuestionsSearchCache.ContainsKey(questionsWithAnswer))
                    interviewData.Levels["#"].QuestionsSearchCache.Add(questionsWithAnswer, new InterviewQuestion(questionsWithAnswer));

                var question = interviewData.Levels["#"].QuestionsSearchCache[questionsWithAnswer]; 
                
                question.Answer = "some answer";
            }
            return interviewData;
        }
    }
}
