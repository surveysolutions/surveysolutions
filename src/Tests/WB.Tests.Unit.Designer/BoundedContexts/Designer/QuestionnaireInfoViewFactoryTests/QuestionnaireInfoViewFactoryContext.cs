﻿using System;
using Main.Core.Documents;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class QuestionnaireInfoViewFactoryContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(string questionnaireId, string questionnaireTitle)
        {
            return Create.QuestionnaireDocument(Guid.Parse(questionnaireId), title : questionnaireTitle);
        }

        protected static QuestionnaireInfoViewFactory CreateQuestionnaireInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> repository = null,
            DesignerDbContext accountsDocumentReader = null)
        {
            var doc = new QuestionnaireDocument();
            var mockedListStorage = new Mock<IPlainStorageAccessor<QuestionnaireListViewItem>>();
            mockedListStorage.SetReturnsDefault(Create.QuestionnaireListViewItem());

            return
                new QuestionnaireInfoViewFactory(repository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(It.IsAny<string>()) == doc),
                                                accountsDocumentReader ?? Create.InMemoryDbContext(),
                                                Mock.Of<IQuestionnaireCompilationVersionService>(), 
                                                Mock.Of<IAttachmentService>(),
                                                Mock.Of<IIdentityService>());
        }
    }
}
