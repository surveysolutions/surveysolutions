﻿using System;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireViewFactoryTests
{
    [TestFixture]
    internal class QuestionnaireViewFactoryTests
    {
        [Test]
        public void When_opening_questionnaire_from_public_folder_Then_should_return_allow_flag()
        {
            Guid questionnaireId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocument();
            var listItem = Create.QuestionnaireListViewItem(isPublic: true, id: questionnaireId);

            var questionnaireStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(q =>
                q.GetById(questionnaireId.FormatGuid()) == questionnaireDocument);

            var inMemoryDbContext = Create.InMemoryDbContext();
            inMemoryDbContext.Questionnaires.Add(listItem);
            inMemoryDbContext.SaveChanges();

            var factory = new QuestionnaireViewFactory(questionnaireStorage, inMemoryDbContext);

            var result = factory.HasUserAccessToQuestionnaire(questionnaireId, userId);

            Assert.True(result);
        }
    }
}
