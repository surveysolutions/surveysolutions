using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionBrief
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }
    public class QuestionnaireInfoFactory : IQuestionnaireInfoFactory
    {
        private IReadSideRepositoryReader<QuestionsAndGroupsCollectionView> questionDetailsReader;
        public QuestionnaireInfoFactory(IReadSideRepositoryReader<QuestionsAndGroupsCollectionView> questionDetailsReader)
        {
            this.questionDetailsReader = questionDetailsReader;
        }

        public QuestionBrief[] GetNumericIntegerQuestionBriefs(string questionnaireId)
        {
            var questionsCollection = this.questionDetailsReader.GetById(questionnaireId);
            return questionsCollection
                .Questions.OfType<NumericDetailsView>()
                .Where(q => q.IsInteger)
                .Select(q => new QuestionBrief
                {
                    Id = q.Id,
                    Title = q.Title
                }).ToArray();
        }

        public QuestionBrief[] GetNotLinkedMultiOptionQuestionBriefs(string questionnaireId)
        {
            var questionsCollection = this.questionDetailsReader.GetById(questionnaireId);
            return questionsCollection
                .Questions.OfType<MultiOptionDetailsView>()
                .Where(q => q.LinkedToQuestionId == null)
                .Select(q => new QuestionBrief
                {
                    Id = q.Id,
                    Title = q.Title
                }).ToArray();
        }

        public QuestionBrief[] GetTextListsQuestionBriefs(string questionnaireId)
        {
            var questionsCollection = this.questionDetailsReader.GetById(questionnaireId);
            return questionsCollection
                .Questions.OfType<TextListDetailsView>()
                .Select(q => new QuestionBrief
                {
                    Id = q.Id,
                    Title = q.Title
                }).ToArray();
        }
    }

    public interface IQuestionnaireInfoFactory
    {
        QuestionBrief[] GetNumericIntegerQuestionBriefs(string questionnaireId);
        QuestionBrief[] GetNotLinkedMultiOptionQuestionBriefs(string questionnaireId);
        QuestionBrief[] GetTextListsQuestionBriefs(string questionnaireId);
    }
}
