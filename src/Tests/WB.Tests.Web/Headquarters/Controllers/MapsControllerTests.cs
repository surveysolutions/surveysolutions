using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models.Maps;

namespace WB.Tests.Web.Headquarters.Controllers;

public class MapsControllerTests
{
    [Test]
    public void Details_when_map_uploaded_by_administrator_should_not_expose_uploaded_by()
    {
        var uploaderId = Guid.NewGuid();
        var controller = CreateController(uploaderId, UserRoles.Administrator, out _);

        var result = controller.Details("map.tif") as ViewResult;

        Assert.That(result, Is.Not.Null);
        var model = result!.Model as MapDetailsModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.UploadedBy, Is.Null);
    }

    [Test]
    public void Details_when_map_uploaded_by_headquarters_should_expose_uploaded_by()
    {
        var uploaderId = Guid.NewGuid();
        var controller = CreateController(uploaderId, UserRoles.Headquarter, out var uploaderName);

        var result = controller.Details("map.tif") as ViewResult;

        Assert.That(result, Is.Not.Null);
        var model = result!.Model as MapDetailsModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.UploadedBy, Is.EqualTo(uploaderName));
    }

    [Test]
    public async Task GetMapContent_should_use_content_hash_as_etag()
    {
        var mapContent = new byte[] { 1, 2, 3, 4 };
        var controller = CreateApiController(mapContent);

        var result = await controller.GetMapContent("map.tif") as FileStreamResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EnableRangeProcessing, Is.True);
        Assert.That(result.EntityTag?.Tag, Is.EqualTo($"\"{Convert.ToHexString(SHA256.HashData(mapContent))}\""));
    }

    private static MapsController CreateController(Guid uploaderId, UserRoles uploaderRole, out string uploaderName)
    {
        uploaderName = "uploader_" + uploaderRole;

        var map = new MapBrowseItem
        {
            Id = "map.tif",
            FileName = "map.tif",
            UploadedBy = uploaderId,
            Wkid = 102100,
        };

        var mapStorage = new Mock<IPlainStorageAccessor<MapBrowseItem>>();
        mapStorage.Setup(x => x.GetById("map.tif")).Returns(map);

        var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.IsSupervisor == false && u.IsObserving == false);

        var uploaderView = new UserViewLite
        {
            PublicKey = uploaderId,
            UserName = uploaderName,
            Roles = new System.Collections.Generic.HashSet<UserRoles> { uploaderRole },
        };
        var userViewFactory = new Mock<IUserViewFactory>();
        userViewFactory.Setup(x => x.GetUser(uploaderId)).Returns(uploaderView);

        var controller = new MapsController(
            mapStorage.Object,
            authorizedUser,
            userViewFactory.Object,
            Mock.Of<IUserRepository>());

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(string.Empty);
        controller.Url = urlHelper.Object;

        return controller;
    }

    private static MapsApiV2Controller CreateApiController(byte[] mapContent)
    {
        var map = new MapBrowseItem
        {
            Id = "map.tif",
            FileName = "map.tif",
            Size = mapContent.Length,
            ImportDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        map.Users.Add(new UserMap { UserName = "interviewer" });

        var mapStorage = new Mock<IPlainStorageAccessor<MapBrowseItem>>();
        mapStorage.Setup(x => x.GetByIdAsync("map.tif")).ReturnsAsync(map);

        var mapRepository = new Mock<IMapStorageService>();
        mapRepository.Setup(x => x.GetMapContentAsync("map.tif")).ReturnsAsync(mapContent);

        var authorizedUser = Mock.Of<IAuthorizedUser>(u => u.UserName == "interviewer" && u.IsSupervisor == false);

        var fileSystemAccessor = new Mock<IFileSystemAccessor>();
        fileSystemAccessor.Setup(x => x.GetFileName("map.tif")).Returns("map.tif");

        return new MapsApiV2Controller(
            mapRepository.Object,
            authorizedUser,
            mapStorage.Object,
            Mock.Of<IUserRepository>(),
            fileSystemAccessor.Object);
    }
}
