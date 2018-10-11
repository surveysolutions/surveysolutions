using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    [NUnit.Framework.TestOf(typeof(QuestionnaireExportStructureFactory))]
    internal class ExportViewFactoryTestsContext
    {
        protected static QuestionnaireExportStructureFactory CreateExportViewFactory(
            IQuestionnaireStorage questionnaireStorage = null)
        {
            return new QuestionnaireExportStructureFactory(
                new MemoryCache(new MemoryCacheOptions()), 
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter( params IQuestionnaireEntity[] chapterChildren)
        {
            var questionnaireDocument= new QuestionnaireDocument
            {
                Children = new List<IQuestionnaireEntity>
                {
                    new Group
                    {
                        Title = "Chapter",
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren ?? Array.Empty<IQuestionnaireEntity>(),
                    }
                }
            };
            return questionnaireDocument;
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Dictionary<string, Guid> variableNameAndQuestionId)
        {
            var questionnaire = new QuestionnaireDocument()
            {
                Children = variableNameAndQuestionId?.Select(x => new NumericQuestion() { VariableName = x.Key, PublicKey = x.Value, QuestionType = QuestionType.Numeric }).
                    ToArray<IQuestionnaireEntity>() ?? Array.Empty<IQuestionnaireEntity>()
            };
            
            return questionnaire;
        }

        protected static InterviewDataExportLevelView GetLevel(InterviewDataExportView interviewDataExportView, Guid[] levelVector)
        {
            return interviewDataExportView.Levels.FirstOrDefault(l => l.LevelVector.SequenceEqual(levelVector));
        }

        protected static InterviewData CreateInterviewData()
        {
            var interviewData = new InterviewData() { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, RosterVector.Empty));
            return interviewData;
        }

        protected static InterviewData CreateInterviewWithAnswers(IEnumerable<Guid> questionsWithAnswers)
        {
            var interviewData = CreateInterviewData();
            foreach (var questionsWithAnswer in questionsWithAnswers)
            {
                if (!interviewData.Levels["#"].QuestionsSearchCache.ContainsKey(questionsWithAnswer))
                    interviewData.Levels["#"].QuestionsSearchCache.Add(questionsWithAnswer, new InterviewEntity
                    {
                        Identity = Create.Identity(questionsWithAnswer)
                    }); 
                var question = interviewData.Levels["#"].QuestionsSearchCache[questionsWithAnswer]; 
                
                question.AsString = "some answer";
            }
            return interviewData;
        }
    }
}
