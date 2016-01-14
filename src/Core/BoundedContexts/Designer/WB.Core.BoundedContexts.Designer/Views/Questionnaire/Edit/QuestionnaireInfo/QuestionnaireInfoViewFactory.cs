﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireInfoView> questionnaireStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersons;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IReadSideRepositoryReader<AccountDocument> accountsDocumentReader;

        public QuestionnaireInfoViewFactory(IReadSideKeyValueStorage<QuestionnaireInfoView> questionnaireStorage,
            IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersons,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IReadSideRepositoryReader<AccountDocument> accountsDocumentReader)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.sharedPersons = sharedPersons;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.accountsDocumentReader = accountsDocumentReader;
        }

        public QuestionnaireInfoView Load(string questionnaireId, Guid personId)
        {
            QuestionnaireInfoView questionnaireInfoView = this.questionnaireStorage.GetById(questionnaireId);

            if (questionnaireInfoView == null)
                return null;

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

            var sharedPersonsList = new List<SharedPerson>();

            QuestionnaireSharedPersons questionnaireSharedPersons = sharedPersons.GetById(questionnaireId);
            if (questionnaireSharedPersons != null)
            {
                sharedPersonsList = questionnaireSharedPersons.SharedPersons;
            }

            if (questionnaireDocument.CreatedBy.HasValue)
            {
                var owner = accountsDocumentReader.GetById(questionnaireDocument.CreatedBy.Value);
                if (owner != null)
                {
                    sharedPersonsList.Insert(0,
                        new SharedPerson() { Email = owner.Email, Id = questionnaireDocument.CreatedBy.Value, IsOwner = true });
                }
            }

            var person = sharedPersonsList.FirstOrDefault(x => x.Id == personId);

            if (person != null)
            {
                questionnaireInfoView.SharedPersons = sharedPersonsList;
                questionnaireInfoView.IsReadOnlyForUser = !person.IsOwner && person.ShareType != 0;
            }
            else
            {
                questionnaireInfoView.IsReadOnlyForUser = true;
            }

            questionnaireInfoView.Macros = questionnaireDocument
                .Macros
                .Select(x => new MacroView { ItemId = x.Key.FormatGuid(), Name = x.Value.Name, Description = x.Value.Description, Content = x.Value.Content })
                .OrderBy(x => x.Name)
                .ToList();

            questionnaireInfoView.LookupTables = questionnaireDocument
                .LookupTables
                .Select(
                    x =>
                        new LookupTableView
                        {
                            ItemId = x.Key.FormatGuid(),
                            Name = x.Value.TableName ?? "",
                            FileName = x.Value.FileName ?? ""
                        })
                .OrderBy(x => x.Name)
                .ToList();
            return questionnaireInfoView;
        }
    }
}