using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.DenormalizerStorageItem;
using Core.Supervisor.Views.Status;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View.Questionnaire;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Tests
{
    public class StatusViewFactoryTests
    {
        [Test]
        public void Load_When_supervisor_is_viewer_status_is_undefined_Then_all_underlining_interviewers_are_returned_with_total_count()
        {

            //arrange
            var supervisorId = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            const int totalCountByInterviewer = 10;

            var items = new InMemoryReadSideRepositoryAccessor<SummaryItem>();
            items.Store(new SummaryItem() { TotalCount = totalCountByInterviewer, ResponsibleSupervisorId = supervisorId, TemplateId = templateId }, Guid.NewGuid());
            
            StatusViewFactory target = CreateStatusViewFactoryWithOneSupervisorAndOneTemplate(supervisorId, templateId, items);

            //act

            var result = target.Load(new StatusViewInputModel() {ViewerId = supervisorId});

            //assert
            Assert.That(result.Items[0].GetCount(templateId), Is.EqualTo(totalCountByInterviewer));
        }

        [Test]
        public void Load_When_supervisor_is_viewer_status_is_undefined_supervisor_has_not_interviewers_Then_empty_list_is_returned()
        {

            //arrange
            var supervisorId = Guid.NewGuid();
            StatusViewFactory target = CreateStatusViewFactory(supervisorId);

            //act

            var result = target.Load(new StatusViewInputModel() { ViewerId = supervisorId });

            //assert
            Assert.That(result.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void Load_When_supervisor_is_viewer_status_is_approved_Then_underlining_interviewrs_are_returned_with_count_by_approved_questionnaries()
        {

            //arrange

            var supervisorId = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            const int approvedCount = 10;

            var items = new InMemoryReadSideRepositoryAccessor<SummaryItem>();

            items.Store(new SummaryItem() { ApprovedCount = approvedCount, ResponsibleSupervisorId = supervisorId, TemplateId = templateId }, Guid.NewGuid());

            StatusViewFactory target = CreateStatusViewFactoryWithOneSupervisorAndOneTemplate(supervisorId, templateId, items);

            //act

            var result =
                target.Load(new StatusViewInputModel()
                    {
                        ViewerId = supervisorId,
                        StatusId = SurveyStatus.Approve.PublicId
                    });

            //assert
            Assert.That(result.Items[0].GetCount(templateId), Is.EqualTo(approvedCount));
        }

        [Test]
        public void Load_When_hq_is_viewer_status_is_undefined_Then_supervisors_are_returned()
        {

            //arrange
            var hqId = Guid.NewGuid();
            const int totalCount = 10;
            var templateId = Guid.NewGuid();
            var items = new InMemoryReadSideRepositoryAccessor<SummaryItem>();

            items.Store(new SummaryItem() { TotalCount = totalCount, ResponsibleSupervisorId = null, TemplateId = templateId }, Guid.NewGuid());
            StatusViewFactory target = CreateStatusViewFactoryWithHQAndOneTemplate(hqId,templateId,items);

            //act

            var result = target.Load(new StatusViewInputModel() {ViewerId = hqId});

            //assert
            Assert.That(result.Items[0].Total, Is.EqualTo(totalCount));
        }

        private StatusViewFactory CreateStatusViewFactory(Guid supervisorId)
        {
            return CreateStatusViewFactoryWithOneSupervisorAndOneTemplate(supervisorId, null,
                                                                          new InMemoryReadSideRepositoryAccessor
                                                                              <SummaryItem>());
        }
        private StatusViewFactory CreateStatusViewFactoryWithHQAndOneTemplate(Guid hqId, Guid? templateId, IQueryableReadSideRepositoryReader<SummaryItem> itemStorage)
        {
            var templateMock = CreateTemplateMock(templateId);
           
            var usersMock = CreateUserMock(hqId,UserRoles.Headquarter);

            return new StatusViewFactory(itemStorage, templateMock.Object, usersMock.Object);
        }

       

        private StatusViewFactory CreateStatusViewFactoryWithOneSupervisorAndOneTemplate(Guid supervisorId, Guid? templateId, IQueryableReadSideRepositoryReader<SummaryItem> itemStorage)
        {
            var usersMock = CreateUserMock(supervisorId, UserRoles.Supervisor);
            var templateMock = CreateTemplateMock(templateId);
            return new StatusViewFactory(itemStorage, templateMock.Object, usersMock.Object);
        }

        private Mock<IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem>> CreateTemplateMock(Guid? templateId)
        {
            var templateMock = new Mock<IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem>>();

            if (templateId.HasValue)
            {
                templateMock.Setup(x => x.QueryAll(It.IsAny<Expression<Func<QuestionnaireBrowseItem, bool>>>()))
                            .Returns(new QuestionnaireBrowseItem[]
                                {
                                    new QuestionnaireBrowseItem(templateId.Value, "smth", DateTime.Now, DateTime.Now,
                                                                null, true)
                                });
            }
            return templateMock;
        }


        private Mock<IQueryableReadSideRepositoryReader<UserDocument>> CreateUserMock(Guid userId, UserRoles role)
        {
            var usersMock = new Mock<IQueryableReadSideRepositoryReader<UserDocument>>();

            usersMock.Setup(x => x.GetById(userId))
                     .Returns(new UserDocument()
                     {
                         PublicKey = userId,
                         UserName = role.ToString(),
                         Roles = new List<UserRoles>() { role }
                     });
            return usersMock;
        }
    }
}
