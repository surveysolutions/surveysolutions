﻿using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Raven.Abstractions.Extensions;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage;
        private readonly IReadSideRepositoryReader<QuestionnaireSharedPersons> sharedPersons;
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IReadSideRepositoryReader<AccountDocument> accountsDocumentReader;

        public QuestionnaireInfoViewFactory(IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage,
            IReadSideRepositoryReader<QuestionnaireSharedPersons> sharedPersons,
            IReadSideRepositoryReader<QuestionnaireDocument> questionnaireDocumentReader, IReadSideRepositoryReader<AccountDocument> accountsDocumentReader)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.sharedPersons = sharedPersons;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.accountsDocumentReader = accountsDocumentReader;
        }

        public QuestionnaireInfoView Load(string questionnaireId)
        {
            QuestionnaireInfoView questionnaireInfoView = this.questionnaireStorage.GetById(questionnaireId);

            QuestionnaireSharedPersons questionnaireSharedPersons = sharedPersons.GetById(questionnaireId);
            if (questionnaireSharedPersons != null)
            {
                questionnaireInfoView.SharedPersons = questionnaireSharedPersons.SharedPersons;
            }
            QuestionnaireDocument questionnaireDocument = this.questionnaireDocumentReader.GetById(questionnaireId);
            questionnaireDocument.Children.TreeToEnumerable(item => item.Children).ForEach(item =>
            {
                if (item is IQuestion)
                {
                    questionnaireInfoView.QuestionsCount++;
                    return;
                }
                var group = item as IGroup;
                if (group != null)
                {
                    if (group.IsRoster)
                    {
                        questionnaireInfoView.RostersCount++;
                    }
                    else
                    {
                        questionnaireInfoView.GroupsCount++;
                    }
                }
            });

            if (questionnaireDocument.CreatedBy.HasValue)
            {
                var owner = accountsDocumentReader.GetById(questionnaireDocument.CreatedBy.Value);
                if (owner != null)
                {
                    questionnaireInfoView.SharedPersons.Insert(0,
                        new SharedPerson() { Email = owner.Email, Id = questionnaireDocument.CreatedBy.Value, IsOwner = true });
                }
            }
            return questionnaireInfoView;
        }
    }
}