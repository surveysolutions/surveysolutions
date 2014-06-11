using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContext.Capi.Synchronization.Views.InterviewMetaInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Androids.Core.Model.Tests.InterviewMetaInfo
{
    [TestFixture]
    public class InterviewMetaInfoFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }


        [Test]
        public void Load_When_record_exists_Then_meta_is_returned()
        {
            // arrange
            Guid interviewId = Guid.NewGuid();//Parse("a2a2a2a2-a2a2-a2a2-a2a2a2a2a2a2");
            Guid featuredItemId = Guid.NewGuid();//Parse("b2a2a2a2-a2a2-a2a2-a2a2a2a2a2a2");
            string title = "title";
            string value = "value";

            List<FeaturedItem> properties = new List<FeaturedItem>()
            {
                new FeaturedItem(featuredItemId, title, value)
            };

            var qDTO = new QuestionnaireDTO(
                interviewId, Guid.NewGuid(), Guid.NewGuid(), InterviewStatus.Completed, properties, 1, true);

            Mock<IFilterableReadSideRepositoryReader<QuestionnaireDTO>> readSideRepositoryReaderMock =
                new Mock<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>();

            readSideRepositoryReaderMock.Setup(x => x.GetById(It.IsAny<string>())).Returns(qDTO);

            InterviewMetaInfoFactory metaFactory = new InterviewMetaInfoFactory(readSideRepositoryReaderMock.Object);
            InterviewMetaInfoInputModel input = new InterviewMetaInfoInputModel(interviewId);

            // act
            WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo info = metaFactory.Load(input);

            // assert
            Assert.That(info.PublicKey, Is.EqualTo(interviewId));
            Assert.That(info.FeaturedQuestionsMeta.Count(), Is.EqualTo(1));
        }
    }
}