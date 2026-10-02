using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code;

namespace WB.Tests.Unit.Designer.Code
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireHelper))]
    public class QuestionnaireHelperTests
    {
        private readonly Guid viewerId = Guid.NewGuid();
        private DesignerIdentityUser viewer;
        private Mock<IQuestionnaireListViewFactory> viewFactory;
        private QuestionnaireListInputModel capturedInput;
        private QuestionnaireHelper helper;

        [SetUp]
        public void SetUp()
        {
            viewer = new DesignerIdentityUser { Id = viewerId, UserName = "me" };
            viewFactory = new Mock<IQuestionnaireListViewFactory>();
            capturedInput = null;
            helper = new QuestionnaireHelper(viewFactory.Object, null, null, null, null, null, null, null, null, null, null);
        }

        private void SetupItems(params IQuestionnaireListItem[] items)
        {
            viewFactory.Setup(f => f.LoadFoldersAndQuestionnaires(It.IsAny<QuestionnaireListInputModel>()))
                .Callback<QuestionnaireListInputModel>(i => capturedInput = i)
                .Returns(new QuestionnaireListView(1, 50, items.Length, items, null));
        }

        private static QuestionnaireListViewItem Item(Guid? owner = null, string creator = "someone", bool deleted = false,
            QuestionnaireListViewFolder folder = null, params SharedPerson[] shared)
            => new QuestionnaireListViewItem
            {
                PublicId = Guid.NewGuid(),
                Title = "Q",
                OwnerId = owner,
                CreatorName = creator,
                IsDeleted = deleted,
                Folder = folder,
                SharedPersons = shared.ToList()
            };

        [Test]
        public void GetQuestionnaires_should_pass_input_filters_to_factory()
        {
            SetupItems();
            var folderId = Guid.NewGuid();

            helper.GetQuestionnaires(viewer, true, QuestionnairesType.Shared, folderId, 3, "Title", -1, "search");

            capturedInput.ViewerId.Should().Be(viewerId);
            capturedInput.IsAdminMode.Should().BeTrue();
            capturedInput.Type.Should().Be(QuestionnairesType.Shared);
            capturedInput.FolderId.Should().Be(folderId);
            capturedInput.Page.Should().Be(3);
            capturedInput.PageSize.Should().Be(GlobalHelper.GridPageItemsCount);
            capturedInput.Order.Should().Be("Title");
            capturedInput.SearchFor.Should().Be("search");
        }

        [Test]
        public void GetQuestionnaires_should_default_page_to_one()
        {
            SetupItems();

            helper.GetQuestionnaires(viewer, false, QuestionnairesType.My, null);

            capturedInput.Page.Should().Be(1);
        }

        [Test]
        public void Owned_questionnaire_should_be_deletable_and_openable()
        {
            var item = Item(owner: viewerId);
            SetupItems(item);

            var model = helper.GetMyQuestionnairesByViewerId(viewer, false).Single();

            model.Id.Should().Be(item.PublicId.FormatGuid());
            model.CanDelete.Should().BeTrue();
            model.CanOpen.Should().BeTrue();
            model.CanSynchronize.Should().BeFalse();
            model.IsFolder.Should().BeFalse();
            model.IsPublic.Should().BeFalse();
        }

        [Test]
        public void Deleted_questionnaire_should_not_be_deletable_or_openable()
        {
            SetupItems(Item(owner: viewerId, deleted: true));

            var model = helper.GetMyQuestionnairesByViewerId(viewer, false).Single();

            model.CanDelete.Should().BeFalse();
            model.CanOpen.Should().BeFalse();
        }

        [Test]
        public void Questionnaire_of_other_owner_should_not_open_unless_shared()
        {
            var notShared = Item(owner: Guid.NewGuid());
            var shared = Item(owner: Guid.NewGuid(), shared: new SharedPerson { UserId = viewerId });
            SetupItems(notShared, shared);

            var models = helper.GetSharedQuestionnairesByViewer(viewer, false, null).ToList();

            models[0].CanOpen.Should().BeFalse();
            models[0].CanDelete.Should().BeFalse();
            models[1].CanOpen.Should().BeTrue();
        }

        [Test]
        public void Admin_should_be_able_to_synchronize()
        {
            SetupItems(Item(owner: viewerId));

            helper.GetMyQuestionnairesByViewerId(viewer, true).Single().CanSynchronize.Should().BeTrue();
        }

        [Test]
        public void CreatedBy_should_be_You_for_viewer_and_placeholder_when_empty()
        {
            SetupItems(Item(creator: "me"), Item(creator: ""), Item(creator: "other"));

            var models = helper.GetMyQuestionnairesByViewerId(viewer, false).ToList();

            models[0].CreatedBy.Should().Be(WB.UI.Designer.Resources.QuestionnaireController.You);
            models[1].CreatedBy.Should().Be(GlobalHelper.EmptyString);
            models[2].CreatedBy.Should().Be("other");
        }

        [Test]
        public void Folder_should_be_mapped_as_folder_with_restricted_permissions()
        {
            var folder = new QuestionnaireListViewFolder(Guid.NewGuid(), "Folder") { CreateDate = new DateTime(2020, 1, 1) };
            SetupItems(folder);

            var model = helper.GetMyQuestionnairesByViewerId(viewer, false).Single();

            model.IsFolder.Should().BeTrue();
            model.Title.Should().Be("Folder");
            model.CreationDate.Should().Be(folder.CreateDate);
            model.CanDelete.Should().BeFalse();
            model.CanCopy.Should().BeFalse();
            model.CanExport.Should().BeFalse();
            model.CanOpen.Should().BeFalse();
            model.CreatedBy.Should().Be(GlobalHelper.EmptyString);
        }

        [Test]
        public void Public_folders_should_be_openable_and_have_location()
        {
            var folder = new QuestionnaireListViewFolder(Guid.NewGuid(), "Folder");
            SetupItems(folder);
            viewFactory.Setup(f => f.LoadFoldersLocation(It.IsAny<IEnumerable<QuestionnaireListViewFolder>>()))
                .Returns(new[] { new QuestionnaireListFolderLocation { PublicId = folder.PublicId, Location = "A / B" } });

            var model = helper.GetQuestionnaires(viewer, false, QuestionnairesType.Public, null).Single();

            model.CanOpen.Should().BeTrue();
            model.IsPublic.Should().BeTrue();
            model.Location.Should().EndWith("A / B");
        }

        [Test]
        public void Public_questionnaire_in_folder_should_get_location_and_admin_can_assign_folder()
        {
            var folder = new QuestionnaireListViewFolder(Guid.NewGuid(), "Folder");
            SetupItems(Item(owner: Guid.NewGuid(), folder: folder));
            viewFactory.Setup(f => f.LoadFoldersLocation(It.IsAny<IEnumerable<QuestionnaireListViewFolder>>()))
                .Returns(new[] { new QuestionnaireListFolderLocation { PublicId = folder.PublicId, Location = "Root" } });

            var model = helper.GetQuestionnaires(viewer, true, QuestionnairesType.Public, null).Single();

            model.CanOpen.Should().BeTrue();
            model.CanAssignFolder.Should().BeTrue();
            model.Location.Should().StartWith("Q").And.EndWith("Root");
        }

        [Test]
        public void Non_public_list_should_not_load_locations()
        {
            SetupItems(Item(owner: viewerId));

            helper.GetMyQuestionnairesByViewerId(viewer, false);

            viewFactory.Verify(f => f.LoadFoldersLocation(It.IsAny<IEnumerable<QuestionnaireListViewFolder>>()), Times.Never);
        }

        [Test]
        public void Result_should_carry_paging_information()
        {
            SetupItems(Item(owner: viewerId));

            var result = helper.GetMyQuestionnairesByViewerId(viewer, false);

            result.TotalCount.Should().Be(1);
            result.PageSize.Should().Be(50);
        }
    }
}

