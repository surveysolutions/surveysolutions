using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public class HqQuestionnaireStorage : QuestionnaireStorage
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireCompositeItem, int> questionnaireItemsWriter;

        public HqQuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository,
            ITranslationStorage translationStorage,
            IQuestionnaireTranslator translator,
            IReadSideRepositoryWriter<QuestionnaireCompositeItem, int> questionnaireItemsWriter,
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService)
            : base(repository, translationStorage, translator, questionOptionsRepository, substitutionService)
        {
            this.questionnaireItemsWriter = questionnaireItemsWriter;
        }

        public override void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            base.StoreQuestionnaire(id, version, questionnaireDocument);

            if (questionnaireDocument.IsDeleted) return;

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, version).ToString();
            
            foreach (var composite in questionnaireDocument.Children.TreeToEnumerable(d => d.Children))
            {
                var question = composite as IQuestion;

                var compositeItem = new QuestionnaireCompositeItem
                {
                    EntityId = composite.PublicKey,
                    ParentId = composite.GetParent()?.PublicKey,
                    QuestionType = question?.QuestionType,
                    QuestionnaireIdentity = questionnaireIdentity,
                    Featured = question?.Featured,
                    QuestionScope = question?.QuestionScope,
                    EntityType = composite.GetEntityType()
                };

                if (question is AbstractQuestion abstractQuestion)
                {
                    compositeItem.LinkedToQuestionId = abstractQuestion.LinkedToQuestionId;
                    compositeItem.LinkedToRosterId = abstractQuestion.LinkedToRosterId;
                    compositeItem.CascadeFromQuestionId = abstractQuestion.CascadeFromQuestionId;
                    compositeItem.IsFilteredCombobox = abstractQuestion.IsFilteredCombobox ?? false;
                    compositeItem.QuestionText = abstractQuestion.QuestionText;
                    compositeItem.VariableLabel = abstractQuestion.VariableLabel;
                    compositeItem.StatExportCaption = abstractQuestion.StataExportCaption;
                }

                if (question?.Answers != null && question.Answers.Any())
                {
                    compositeItem.Answers = question.Answers.Select(a => new QuestionnaireCompositeItemAnswer
                    {
                        Value = a.AnswerValue,
                        Text = a.AnswerText,
                        AnswerCode = a.AnswerCode,
                        Parent = a.ParentValue,
                        ParentCode = a.ParentCode
                    }).ToList();
                }

                questionnaireItemsWriter.Store(compositeItem);
            }
        }
    }
}
