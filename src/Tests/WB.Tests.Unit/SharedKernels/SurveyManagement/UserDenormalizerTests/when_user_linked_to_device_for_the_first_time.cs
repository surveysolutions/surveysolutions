using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserDenormalizerTests
{
    internal class when_user_linked_to_device_for_the_first_time : UserDenormalizerTests
    {
        Establish context = () =>
        {
            user = CreateUser(userId);

            userDocumentWriterMock = new Mock<IReadSideRepositoryWriter<UserDocument>>();

            userDocumentWriterMock
                .Setup(x => x.GetById(userId.FormatGuid()))
                .Returns(user);

            userDocumentWriterMock
                .Setup(x => x.Store(Moq.It.IsAny<UserDocument>(), Moq.It.IsAny<string>()))
                .Callback((UserDocument u, string id) =>
                {
                    savedUser = u;
                    savedUserId = id;
                });

            userLinkedToDeviceEvent = Create.Event.UserLinkedToDevice(userId, deviceId, eventTimeStamp);
            userDenormalizer = CreatevUserDenormalizer(userDocumentWriterMock.Object);
        };

        Because of = () => 
            userDenormalizer.Handle(userLinkedToDeviceEvent);

        It should_save_user_with_DeviceId_specified = () =>
            user.DeviceId.ShouldEqual(deviceId);

        It should_save_user_with_one_row_in_history = () =>
            user.DeviceChangingHistory.Count.ShouldEqual(1);

        It should_save_user_with_specified_DeviceId_in_history = () =>
            user.DeviceChangingHistory.First().DeviceId.ShouldEqual(deviceId);

        It should_save_user_with_specified_DeviceId_in_history_with_Date_specified = () =>
            user.DeviceChangingHistory.First().Date.ShouldEqual(eventTimeStamp);

        It should_save_user_with_id_as_formatted_userId = () => 
            savedUserId.ShouldEqual(userId.FormatGuid());

        private static UserDenormalizer userDenormalizer;
        private static UserDocument user;
        private static UserDocument savedUser;
        private static string savedUserId;
        private static Mock<IReadSideRepositoryWriter<UserDocument>> userDocumentWriterMock;
        private static IPublishedEvent<UserLinkedToDevice> userLinkedToDeviceEvent;
        private static string deviceId = "DeviceId";
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static DateTime eventTimeStamp = new DateTime(2014, 4, 20);
    }
}