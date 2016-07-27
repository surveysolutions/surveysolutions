﻿using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_with_translations : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            sourceQuestionnaireId = Create.Entity.QuestionnaireIdentity(questionnaireId, 4);

            translations = new TestPlainStorage<TranslationInstance>();
            translations.Store(Create.Entity.TranslationInstance(
                questionnaireId: sourceQuestionnaireId,
                value: "translation")
            , 1);

            var questionnaireDocument = Create.Entity.QuestionnaireDocument();
            questionnaireDocument.PublicKey = sourceQuestionnaireId.QuestionnaireId;

            var plainQuestionnaireRepositoryMock = 
                Mock.Get(Mock.Of<IQuestionnaireStorage>(_ => 
                    _.GetQuestionnaireDocument(sourceQuestionnaireId.QuestionnaireId, sourceQuestionnaireId.Version) == questionnaireDocument));

            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
              = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                  id: sourceQuestionnaireId.ToString(), entity: Create.Entity.QuestionnaireBrowseItem(questionnaireIdentity: sourceQuestionnaireId));


            questionnaire = Create.AggregateRoot.Questionnaire(
                translationsStorage: translations,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage,
                questionnaireStorage: plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () => questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(sourceQuestionnaireId, newQuestionnaireVersion: targetQuestionnaireVersion));

        It should_store_copy_of_translation = () => 
            translations.Query(_ => _.Count(x => 
                x.QuestionnaireId.QuestionnaireId == sourceQuestionnaireId.QuestionnaireId && 
                x.QuestionnaireId.Version == targetQuestionnaireVersion)).ShouldEqual(1);

        static Questionnaire questionnaire;
        static QuestionnaireIdentity sourceQuestionnaireId;
        static IPlainStorageAccessor<TranslationInstance> translations;
        static int targetQuestionnaireVersion = 5;
    }
}