using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Services.QuestionnaireLists
{
    public class when_query_for_questionnaire_list
    {
        private IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private IPlainStorageAccessor<QuestionnaireListViewFolder> publicFoldersStorage;

        [SetUp]
        public void Context()
        {
            this.questionnaireListViewItemStorage = Abc.Create.Storage.InMemoryPlainStorage<QuestionnaireListViewItem>();
            this.publicFoldersStorage = Abc.Create.Storage.InMemoryPlainStorage<QuestionnaireListViewFolder>();

            this.Subject = new QuestionnaireListViewFactory(this.questionnaireListViewItemStorage, this.publicFoldersStorage);

            // my
            this.questionnaireListViewItemStorage.Store(Create.QuestionnaireListViewItem(Id.gA, isPublic: false, createdBy: Id.g1), Id.gA);

            // shared questionnaires
            this.questionnaireListViewItemStorage.Store(Create.QuestionnaireListViewItem(Id.gB, isPublic: false,
                createdBy: Id.g2,
                sharedPersons: new[] {Create.SharedPerson(Id.g1)}), Id.gB);

            // public
            this.questionnaireListViewItemStorage.Store(Create.QuestionnaireListViewItem(Id.gC, isPublic: true, createdBy: Id.g3), Id.gC);
        }

        private QuestionnaireListViewFactory Subject { get; set; }

        [Test]
        public void should_show_only_my_questionnaires()
        {
            var results = Subject.Load(new QuestionnaireListInputModel
            {
                Type = QuestionnairesType.My,
                IsAdminMode = false,
                ViewerId = Id.g1
            });

            Assert.That(results.TotalCount, Is.EqualTo(1));

            Assert.That(results.Items.Single().PublicId, Is.EqualTo(Id.gA));
        }

        [Test]
        public void should_show_only_shared_questionnaires()
        {
            var results = Subject.Load(new QuestionnaireListInputModel
            {
                Type = QuestionnairesType.Shared,
                IsAdminMode = false,
                ViewerId = Id.g1
            });

            Assert.That(results.TotalCount, Is.EqualTo(1));

            Assert.That(results.Items.Single().PublicId, Is.EqualTo(Id.gB));
        }

        [Test]
        public void should_show_only_my_and_shared_questionnaires()
        {
            var results = Subject.Load(new QuestionnaireListInputModel
            {
                Type = QuestionnairesType.My | QuestionnairesType.Shared,
                IsAdminMode = false,
                ViewerId = Id.g1
            });

            Assert.That(results.TotalCount, Is.EqualTo(2));

            Assert.That(results.Items, Has.One.Property(nameof(QuestionnaireListViewItem.PublicId)).EqualTo(Id.gA));
            Assert.That(results.Items, Has.One.Property(nameof(QuestionnaireListViewItem.PublicId)).EqualTo(Id.gB));
        }

        [Test]
        public void should_show_only_public_questionnaires()
        {
            var results = Subject.Load(new QuestionnaireListInputModel
            {
                Type = QuestionnairesType.Public,
                IsAdminMode = false,
                ViewerId = Id.g1
            });

            Assert.That(results.TotalCount, Is.EqualTo(1));
            
            Assert.That(results.Items, Has.One.Property(nameof(QuestionnaireListViewItem.PublicId)).EqualTo(Id.gC));
        }

        [Test] public void should_show_all_questionnaires_for_admin()
        {
            var results = Subject.Load(new QuestionnaireListInputModel
            {
                Type = QuestionnairesType.Public,
                IsAdminMode = true,
                ViewerId = Id.g1
            });

            Assert.That(results.TotalCount, Is.EqualTo(3));
            
            Assert.That(results.Items, Has.One.Property(nameof(QuestionnaireListViewItem.PublicId)).EqualTo(Id.gA));
            Assert.That(results.Items, Has.One.Property(nameof(QuestionnaireListViewItem.PublicId)).EqualTo(Id.gB));
            Assert.That(results.Items, Has.One.Property(nameof(QuestionnaireListViewItem.PublicId)).EqualTo(Id.gC));
        }
    }
}
