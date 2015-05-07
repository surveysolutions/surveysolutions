using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserDenormalizerTests
{
    internal class when_user_linked_to_device_for_the_second_time : UserDenormalizerTests
    {
        Establish context = () =>
        {
            user = CreateUser(userId, history: new HashSet<DeviceInfo>
            {
                CreateDeviceInfo(deviceIdHistory, eventTimeStampHistory)   
            });

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

        It should_save_user_with_two_rows_in_history = () =>
            user.DeviceChangingHistory.Count.ShouldEqual(2);

        It should_save_user_with_intact_DeviceId_in_the_first_history_row = () =>
            user.DeviceChangingHistory.First().DeviceId.ShouldEqual(deviceIdHistory);

        It should_save_user_with_intact_Date_in_history_with_Date_specified = () =>
            user.DeviceChangingHistory.First().Date.ShouldEqual(eventTimeStampHistory);

        It should_save_user_with_specified_DeviceId_in_history = () =>
           user.DeviceChangingHistory.Second().DeviceId.ShouldEqual(deviceId);

        It should_save_user_with_specified_DeviceId_in_history_with_Date_specified = () =>
            user.DeviceChangingHistory.Second().Date.ShouldEqual(eventTimeStamp);

        It should_save_user_with_id_as_formatted_userId = () =>
            savedUserId.ShouldEqual(userId.FormatGuid());

        private static UserDenormalizer userDenormalizer;
        private static UserDocument user;
        private static UserDocument savedUser;
        private static string savedUserId;
        private static Mock<IReadSideRepositoryWriter<UserDocument>> userDocumentWriterMock;
        private static IPublishedEvent<UserLinkedToDevice> userLinkedToDeviceEvent;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string deviceIdHistory = "DeviceIdHistory";
        private static DateTime eventTimeStampHistory = new DateTime(2010, 5, 13);
        private static string deviceId = "DeviceId";
        private static DateTime eventTimeStamp = new DateTime(2014, 4, 20);
    }
}