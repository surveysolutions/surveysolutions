using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view_for_historic_revision : QuestionnaireInfoViewFactoryContext
    {
        private QuestionnaireInfoView view;
        protected new static QuestionnaireRevision questionnaireId = Create.QuestionnaireRevision(Id.g1, Id.gA);

        [SetUp]
        public void Because()
        {
            var repositoryMock = new Mock<IDesignerQuestionnaireStorage>();

            repositoryMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(CreateQuestionnaireDocument(questionnaireId.QuestionnaireId.FormatGuid(), "Hey"));

            var dbContext = Create.InMemoryDbContext();
            dbContext.Questionnaires.Add(Create.QuestionnaireListViewItem(id: questionnaireId.QuestionnaireId));
            dbContext.SaveChanges();

            var factory = CreateQuestionnaireInfoViewFactory(repositoryMock.Object, dbContext);

            this.view = factory.Load(questionnaireId, Guid.Empty);
        }

        [Test]
        public void should_set_readonly_flag() => ClassicAssert.True(view.IsReadOnlyForUser);

        [Test]
        public void should_set_empty_shared_list() => ClassicAssert.False(view.SharedPersons.Any());
    }
}
