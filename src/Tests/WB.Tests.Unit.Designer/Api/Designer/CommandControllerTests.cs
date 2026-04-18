using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Api.Designer
{
    [TestFixture]
    [TestOf(typeof(CommandController))]
    public class CommandControllerTests
    {
        #region Factory

        private static CommandController CreateController(
            ICommandService commandService = null,
            DesignerDbContext dbContext = null,
            ICommandInflater commandInflater = null,
            ILookupTableService lookupTableService = null,
            IAttachmentService attachmentService = null,
            IDesignerTranslationService translationsService = null,
            IReusableCategoriesService reusableCategoriesService = null,
            IFileSystemAccessor fileSystemAccessor = null)
        {
            var controller = new CommandController(
                commandService ?? Mock.Of<ICommandService>(),
                dbContext ?? Create.InMemoryDbContext(),
                Mock.Of<ILogger<CommandController>>(),
                commandInflater ?? Mock.Of<ICommandInflater>(),
                lookupTableService ?? Mock.Of<ILookupTableService>(),
                attachmentService ?? Mock.Of<IAttachmentService>(),
                translationsService ?? Mock.Of<IDesignerTranslationService>(),
                reusableCategoriesService ?? Mock.Of<IReusableCategoriesService>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static string SerializeCommand(object command) => JsonConvert.SerializeObject(command);

        private static string ValidUpdateQuestionnaireJson()
        {
            var cmd = new UpdateQuestionnaire(
                Guid.NewGuid(), "Title", "var1", false, false, "English", Guid.NewGuid(), false);
            return SerializeCommand(cmd);
        }

        private static string ValidAddOrUpdateAttachmentJson()
        {
            var cmd = new AddOrUpdateAttachment(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "attach.png", "old-content-id", Guid.NewGuid());
            return SerializeCommand(cmd);
        }

        private static (string json, Guid questionnaireId, Guid translationId) ValidAddOrUpdateTranslationJson()
        {
            var questionnaireId = Guid.NewGuid();
            var translationId = Guid.NewGuid();
            var cmd = new AddOrUpdateTranslation(
                questionnaireId, Guid.NewGuid(), translationId, "English", null);
            return (SerializeCommand(cmd), questionnaireId, translationId);
        }

        private static (string json, Guid questionnaireId, Guid categoriesId) ValidAddOrUpdateCategoriesJson()
        {
            var questionnaireId = Guid.NewGuid();
            var categoriesId = Guid.NewGuid();
            var cmd = new AddOrUpdateCategories(
                questionnaireId, Guid.NewGuid(), categoriesId, "Categories", null);
            return (SerializeCommand(cmd), questionnaireId, categoriesId);
        }

        private static Mock<IFormFile> CreateMockFormFile(string fileName = "file.xlsx",
            string contentType = "application/octet-stream")
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return mockFile;
        }

        private static int StatusCodeOf(IActionResult result) =>
            ((JsonResult)result).StatusCode!.Value;

        private static string MessageOf(IActionResult result)
        {
            var value = ((JsonResult)result).Value;
            var prop = value!.GetType().GetProperty("message");
            return prop?.GetValue(value)?.ToString() ?? string.Empty;
        }

        #endregion

        #region Deserialize

        [Test]
        public void Deserialize_unknown_command_type_throws_CommandDeserializationException()
        {
            var controller = CreateController();

            Assert.Throws<CommandDeserializationException>(
                () => controller.Deserialize("CompletelyUnknownCommandType", "{}"));
        }

        [Test]
        public void Deserialize_malformed_json_throws_CommandDeserializationException()
        {
            var controller = CreateController();

            Assert.Throws<CommandDeserializationException>(
                () => controller.Deserialize("UpdateQuestionnaire", "not { valid } json [[["));
        }

        [Test]
        public void Deserialize_valid_json_returns_correct_command_instance()
        {
            var controller = CreateController();
            var json = ValidUpdateQuestionnaireJson();

            var result = controller.Deserialize("UpdateQuestionnaire", json);

            Assert.That(result, Is.InstanceOf<UpdateQuestionnaire>());
        }

        [Test]
        public void Deserialize_null_serialized_command_throws_CommandDeserializationException()
        {
            var controller = CreateController();

            Assert.Throws<CommandDeserializationException>(
                () => controller.Deserialize("UpdateQuestionnaire", null!));
        }

        [Test]
        public void Deserialize_all_known_command_types_can_be_resolved()
        {
            var controller = CreateController();

            // Every registered key should be resolvable (found in KnownCommandTypes).
            // Deserializing `{}` may either succeed (returning a default instance) or throw a
            // CommandDeserializationException for a reason OTHER than "type not supported"
            // (e.g. the result is null for some types). Either outcome proves the type lookup worked.
            var knownKeys = new[]
            {
                "UpdateQuestionnaire", "UpdateGroup", "AddGroup", "DeleteGroup", "MoveGroup",
                "AddDefaultTypeQuestion", "DeleteQuestion", "MoveQuestion",
                "AddSharedPersonToQuestionnaire", "RemoveSharedPersonFromQuestionnaire",
                "ReplaceTexts",
                "UpdateTextQuestion", "UpdateNumericQuestion", "UpdateDateTimeQuestion",
                "UpdateTextListQuestion", "UpdateQRBarcodeQuestion", "UpdateMultimediaQuestion",
                "UpdateMultiOptionQuestion", "UpdateSingleOptionQuestion",
                "UpdateGpsCoordinatesQuestion", "UpdateFilteredComboboxOptions",
                "UpdateAreaQuestion", "UpdateAudioQuestion",
                "ReplaceOptionsWithClassification", "MigrateToNewVersion",
                "AddStaticText", "UpdateStaticText", "DeleteStaticText", "MoveStaticText",
                "AddVariable", "UpdateVariable", "DeleteVariable", "MoveVariable",
                "PasteAfter", "PasteInto",
                "AddMacro", "UpdateMacro", "DeleteMacro",
                "AddLookupTable", "UpdateLookupTable", "DeleteLookupTable",
                "AddOrUpdateAttachment", "DeleteAttachment",
                "AddOrUpdateTranslation", "DeleteTranslation", "SetDefaultTranslation",
                "SwitchToTranslation",
                "UpdateMetadata",
                "PassOwnershipFromQuestionnaire",
                "AddOrUpdateCategories", "DeleteCategories",
                "AddCriticalRule", "UpdateCriticalRule", "DeleteCriticalRule"
            };

            foreach (var key in knownKeys)
            {
                // The call must NOT throw with "is not supported" — that would mean the type
                // was not registered in KnownCommandTypes.
                // It is fine if it succeeds (type found and {} produced a valid instance)
                // or throws CommandDeserializationException for a different reason
                // (e.g. the deserialized object came back null for that type).
                CommandDeserializationException caughtEx = null;
                try
                {
                    controller.Deserialize(key, "{}");
                }
                catch (CommandDeserializationException e)
                {
                    caughtEx = e;
                }

                if (caughtEx != null)
                {
                    Assert.That(caughtEx.Message, Does.Not.Contain("is not supported"),
                        $"Key '{key}' was not found in KnownCommandTypes");
                }
                // else: deserialization succeeded — the type was found and {} was deserializable
            }
        }

        #endregion

        #region UpdateAttachment

        [Test]
        public async Task UpdateAttachment_null_model_returns_406()
        {
            var controller = CreateController();

            var result = await controller.UpdateAttachment(null!);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
        }

        [Test]
        public async Task UpdateAttachment_null_command_in_model_returns_406()
        {
            var controller = CreateController();
            var model = new CommandController.AttachmentModel { Command = null };

            var result = await controller.UpdateAttachment(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
        }

        [Test]
        public async Task UpdateAttachment_malformed_command_json_returns_406_with_generic_message()
        {
            var controller = CreateController();
            var model = new CommandController.AttachmentModel { Command = "not valid json {{{" };

            var result = await controller.UpdateAttachment(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            Assert.That(MessageOf(result), Is.EqualTo("Invalid command"));
        }

        [Test]
        public async Task UpdateAttachment_no_file_no_old_attachment_id_returns_406_with_original_message()
        {
            // No file + no OldAttachmentId → ArgumentException thrown inside handler
            var cmd = new AddOrUpdateAttachment(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "name", "cid", oldAttachmentId: null);
            var model = new CommandController.AttachmentModel
            {
                Command = SerializeCommand(cmd),
                File = null
            };
            var controller = CreateController();

            var result = await controller.UpdateAttachment(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            // Message comes from the ArgumentException, not the generic masking message
            Assert.That(MessageOf(result), Is.Not.EqualTo("Invalid command"));
        }

        [Test]
        public async Task UpdateAttachment_no_file_content_id_not_found_returns_406_with_original_message()
        {
            var json = ValidAddOrUpdateAttachmentJson();
            var model = new CommandController.AttachmentModel
            {
                Command = json,
                File = null
            };

            var attachmentService = new Mock<IAttachmentService>();
            attachmentService
                .Setup(s => s.GetAttachmentContentId(It.IsAny<Guid>()))
                .Returns(default(string));

            var controller = CreateController(attachmentService: attachmentService.Object);

            var result = await controller.UpdateAttachment(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
        }

        [Test]
        public async Task UpdateAttachment_with_file_calls_attachment_service_and_returns_ok()
        {
            var json = ValidAddOrUpdateAttachmentJson();
            var mockFile = CreateMockFormFile("photo.png", "image/png");

            var model = new CommandController.AttachmentModel
            {
                Command = json,
                File = mockFile.Object
            };

            var attachmentService = new Mock<IAttachmentService>();
            attachmentService.Setup(s => s.CreateAttachmentContentId(It.IsAny<byte[]>()))
                .Returns("new-content-id");

            var controller = CreateController(attachmentService: attachmentService.Object);

            var result = await controller.UpdateAttachment(model);

            Assert.That(result, Is.InstanceOf<OkResult>());
            attachmentService.Verify(s =>
                s.SaveContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
            attachmentService.Verify(s =>
                s.SaveMeta(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task UpdateAttachment_no_file_with_valid_old_attachment_calls_process_and_returns_ok()
        {
            var json = ValidAddOrUpdateAttachmentJson();
            var model = new CommandController.AttachmentModel { Command = json, File = null };

            var attachmentService = new Mock<IAttachmentService>();
            attachmentService
                .Setup(s => s.GetAttachmentContentId(It.IsAny<Guid>()))
                .Returns("existing-content-id");

            var controller = CreateController(attachmentService: attachmentService.Object);

            var result = await controller.UpdateAttachment(model);

            Assert.That(result, Is.InstanceOf<OkResult>());
            attachmentService.Verify(s =>
                s.SaveMeta(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region UpdateLookupTable

        [Test]
        public async Task UpdateLookupTable_non_multipart_content_type_returns_415()
        {
            var controller = CreateController();
            controller.ControllerContext.HttpContext.Request.ContentType = "application/json";

            var result = await controller.UpdateLookupTable();

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.UnsupportedMediaType));
        }

        [Test]
        public async Task UpdateLookupTable_null_content_type_returns_415()
        {
            var controller = CreateController();
            controller.ControllerContext.HttpContext.Request.ContentType = null;

            var result = await controller.UpdateLookupTable();

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.UnsupportedMediaType));
        }

        #endregion

        #region Post

        [Test]
        public void Post_null_model_throws_InvalidOperationException()
        {
            var controller = CreateController();

            Assert.ThrowsAsync<InvalidOperationException>(() => controller.Post(null!));
        }

        [Test]
        public void Post_null_type_throws_InvalidOperationException()
        {
            var controller = CreateController();
            var model = new CommandController.CommandExecutionModel { Type = null, Command = "{}" };

            Assert.ThrowsAsync<InvalidOperationException>(() => controller.Post(model));
        }

        [Test]
        public void Post_null_command_throws_InvalidOperationException()
        {
            var controller = CreateController();
            var model = new CommandController.CommandExecutionModel { Type = "UpdateQuestionnaire", Command = null };

            Assert.ThrowsAsync<InvalidOperationException>(() => controller.Post(model));
        }

        [Test]
        public async Task Post_unknown_command_type_returns_406_with_generic_message()
        {
            var controller = CreateController();
            var model = new CommandController.CommandExecutionModel
            {
                Type = "NoSuchCommandType",
                Command = "{}"
            };

            var result = await controller.Post(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            Assert.That(MessageOf(result), Is.EqualTo("Invalid command"));
        }

        [Test]
        public async Task Post_malformed_json_returns_406_with_generic_message()
        {
            var controller = CreateController();
            var model = new CommandController.CommandExecutionModel
            {
                Type = "UpdateQuestionnaire",
                Command = "not { valid } json [[["
            };

            var result = await controller.Post(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            Assert.That(MessageOf(result), Is.EqualTo("Invalid command"));
        }

        [Test]
        public async Task Post_valid_command_executes_and_returns_ok()
        {
            var commandService = new Mock<ICommandService>();
            var controller = CreateController(commandService: commandService.Object);

            var model = new CommandController.CommandExecutionModel
            {
                Type = "UpdateQuestionnaire",
                Command = ValidUpdateQuestionnaireJson()
            };

            var result = await controller.Post(model);

            Assert.That(result, Is.InstanceOf<OkResult>());
            commandService.Verify(s => s.Execute(It.IsAny<ICommand>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Post_command_service_throws_argument_exception_returns_406_with_original_message()
        {
            const string errorMessage = "Business rule violated";
            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(s => s.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(new ArgumentException(errorMessage));

            var controller = CreateController(commandService: commandService.Object);
            var model = new CommandController.CommandExecutionModel
            {
                Type = "UpdateQuestionnaire",
                Command = ValidUpdateQuestionnaireJson()
            };

            var result = await controller.Post(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            Assert.That(MessageOf(result), Is.EqualTo(errorMessage));
        }

        [Test]
        public async Task Post_command_inflater_throws_forbidden_exception_returns_403()
        {
            var commandInflater = new Mock<ICommandInflater>();
            commandInflater
                .Setup(i => i.PrepareDeserializedCommandForExecution(It.IsAny<ICommand>()))
                .Throws(new CommandInflaitingException(CommandInflatingExceptionType.Forbidden, "Access denied"));

            var controller = CreateController(commandInflater: commandInflater.Object);
            var model = new CommandController.CommandExecutionModel
            {
                Type = "UpdateQuestionnaire",
                Command = ValidUpdateQuestionnaireJson()
            };

            var result = await controller.Post(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo(StatusCodes.Status403Forbidden));
        }

        [Test]
        public async Task Post_command_service_throws_questionnaire_domain_exception_returns_406()
        {
            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(s => s.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(new QuestionnaireException(DomainExceptionType.Undefined, "Domain error"));

            var controller = CreateController(commandService: commandService.Object);
            var model = new CommandController.CommandExecutionModel
            {
                Type = "UpdateQuestionnaire",
                Command = ValidUpdateQuestionnaireJson()
            };

            var result = await controller.Post(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo(StatusCodes.Status406NotAcceptable));
        }

        [Test]
        public async Task Post_command_service_throws_forbidden_domain_exception_returns_403()
        {
            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(s => s.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(new QuestionnaireException(DomainExceptionType.DoesNotHavePermissionsForEdit, "Forbidden"));

            var controller = CreateController(commandService: commandService.Object);
            var model = new CommandController.CommandExecutionModel
            {
                Type = "UpdateQuestionnaire",
                Command = ValidUpdateQuestionnaireJson()
            };

            var result = await controller.Post(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo(StatusCodes.Status403Forbidden));
        }

        #endregion

        #region UpdateTranslation

        [Test]
        public async Task UpdateTranslation_null_model_returns_406()
        {
            var controller = CreateController();

            var result = await controller.UpdateTranslation(null!);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
        }

        [Test]
        public async Task UpdateTranslation_null_command_in_model_returns_406()
        {
            var controller = CreateController();
            var model = new CommandController.FileModel { Command = null };

            var result = await controller.UpdateTranslation(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
        }

        [Test]
        public async Task UpdateTranslation_malformed_command_json_returns_406_with_generic_message()
        {
            var controller = CreateController();
            var model = new CommandController.FileModel { Command = "invalid json {{{{" };

            var result = await controller.UpdateTranslation(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            Assert.That(MessageOf(result), Is.EqualTo("Invalid command"));
        }

        [Test]
        public async Task UpdateTranslation_invalid_file_exception_returns_406_with_error_messages()
        {
            const string header = "Invalid translation file";
            const string detail = "Row 3 is bad";
            var translationsService = new Mock<IDesignerTranslationService>();
            translationsService
                .Setup(s => s.Store(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<byte[]>()))
                .Throws(new InvalidFileException(header)
                {
                    FoundErrors = new List<ImportValidationError>
                    {
                        new ImportValidationError { Message = detail }
                    }
                });

            var mockFile = CreateMockFormFile("translation.xlsx");
            var (json, _, _) = ValidAddOrUpdateTranslationJson();
            var model = new CommandController.FileModel { Command = json, File = mockFile.Object };

            var controller = CreateController(translationsService: translationsService.Object);

            var result = await controller.UpdateTranslation(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            var message = MessageOf(result);
            Assert.That(message, Does.Contain(header));
            Assert.That(message, Does.Contain(detail));
        }

        [Test]
        public async Task UpdateTranslation_without_file_processes_command_and_returns_response()
        {
            var (json, _, _) = ValidAddOrUpdateTranslationJson();
            var model = new CommandController.FileModel { Command = json, File = null };
            var controller = CreateController();

            var result = await controller.UpdateTranslation(model);

            // No file → returns ProcessCommand response (Ok)
            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task UpdateTranslation_with_file_stores_translation_and_returns_count_message()
        {
            var (json, questionnaireId, translationId) = ValidAddOrUpdateTranslationJson();
            const int translationCount = 5;

            var translationsService = new Mock<IDesignerTranslationService>();
            translationsService
                .Setup(s => s.Count(questionnaireId, translationId))
                .Returns(translationCount);

            var mockFile = CreateMockFormFile("translation.xlsx");
            var model = new CommandController.FileModel { Command = json, File = mockFile.Object };

            var controller = CreateController(translationsService: translationsService.Object);

            var result = await controller.UpdateTranslation(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            translationsService.Verify(s =>
                s.Store(questionnaireId, translationId, It.IsAny<byte[]>()), Times.Once);
        }

        #endregion

        #region UpdateCategories

        [Test]
        public async Task UpdateCategories_null_model_returns_406()
        {
            var controller = CreateController();

            var result = await controller.UpdateCategories(null!);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
        }

        [Test]
        public async Task UpdateCategories_null_command_in_model_returns_406()
        {
            var controller = CreateController();
            var model = new CommandController.FileModel { Command = null };

            var result = await controller.UpdateCategories(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
        }

        [Test]
        public async Task UpdateCategories_malformed_command_json_returns_406_with_generic_message()
        {
            var controller = CreateController();
            var model = new CommandController.FileModel { Command = "INVALID{{{{" };

            var result = await controller.UpdateCategories(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            Assert.That(MessageOf(result), Is.EqualTo("Invalid command"));
        }

        [Test]
        public async Task UpdateCategories_invalid_file_extension_returns_406_with_original_message()
        {
            var (json, _, _) = ValidAddOrUpdateCategoriesJson();
            var mockFile = CreateMockFormFile("data.csv", "text/csv");

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(f => f.GetFileExtension("data.csv")).Returns(".csv");

            var model = new CommandController.FileModel { Command = json, File = mockFile.Object };
            var controller = CreateController(fileSystemAccessor: fileSystemAccessor.Object);

            var result = await controller.UpdateCategories(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            // Original ArgumentException message is returned (not masked)
            Assert.That(MessageOf(result), Is.Not.EqualTo("Invalid command"));
        }

        [Test]
        public async Task UpdateCategories_invalid_file_exception_returns_406_with_error_messages()
        {
            const string header = "Invalid categories file";
            const string detail = "Value in row 2 is not numeric";

            var reusableCategoriesService = new Mock<IReusableCategoriesService>();
            reusableCategoriesService
                .Setup(s => s.Store(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<CategoriesFileType>()))
                .Throws(new InvalidFileException(header)
                {
                    FoundErrors = new List<ImportValidationError>
                    {
                        new ImportValidationError { Message = detail }
                    }
                });

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(f => f.GetFileExtension(It.IsAny<string>())).Returns(".xlsx");

            var mockFile = CreateMockFormFile("categories.xlsx");
            var (json, _, _) = ValidAddOrUpdateCategoriesJson();
            var model = new CommandController.FileModel { Command = json, File = mockFile.Object };

            var controller = CreateController(
                reusableCategoriesService: reusableCategoriesService.Object,
                fileSystemAccessor: fileSystemAccessor.Object);

            var result = await controller.UpdateCategories(model);

            Assert.That(StatusCodeOf(result), Is.EqualTo((int)HttpStatusCode.NotAcceptable));
            var message = MessageOf(result);
            Assert.That(message, Does.Contain(header));
            Assert.That(message, Does.Contain(detail));
        }

        [Test]
        public async Task UpdateCategories_without_file_processes_command_and_returns_response()
        {
            var (json, _, _) = ValidAddOrUpdateCategoriesJson();
            var model = new CommandController.FileModel { Command = json, File = null };
            var controller = CreateController();

            var result = await controller.UpdateCategories(model);

            // No file → returns ProcessCommand response directly (Ok)
            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task UpdateCategories_with_valid_excel_file_stores_categories_and_returns_count_message()
        {
            var (json, questionnaireId, categoriesId) = ValidAddOrUpdateCategoriesJson();

            var reusableCategoriesService = new Mock<IReusableCategoriesService>();
            reusableCategoriesService
                .Setup(s => s.GetCategoriesById(questionnaireId, categoriesId))
                .Returns(Enumerable.Range(1, 3).Select(_ => new CategoriesItem()).AsQueryable());

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(f => f.GetFileExtension(It.IsAny<string>())).Returns(".xlsx");

            var mockFile = CreateMockFormFile("categories.xlsx");
            var model = new CommandController.FileModel { Command = json, File = mockFile.Object };

            var controller = CreateController(
                reusableCategoriesService: reusableCategoriesService.Object,
                fileSystemAccessor: fileSystemAccessor.Object);

            var result = await controller.UpdateCategories(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            reusableCategoriesService.Verify(s =>
                s.Store(questionnaireId, categoriesId, It.IsAny<Stream>(), CategoriesFileType.Excel), Times.Once);
        }

        #endregion
    }
}









