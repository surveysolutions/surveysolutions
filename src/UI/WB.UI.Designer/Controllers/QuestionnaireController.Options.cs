using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code.Vue;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        public ActionResult<EditOptionsViewModel> GetCategoryOptions(QuestionnaireRevision id, Guid categoriesId)
        {
            var categoriesView = this.questionnaireInfoFactory.GetCategoriesView(id, categoriesId);

            if (categoriesView == null)
                return NotFound();

            var questionnaireId = id.OriginalQuestionnaireId ?? id.QuestionnaireId;
            var categories =
                this.categoriesService.GetCategoriesById(questionnaireId, categoriesId).
                    Select(
                        option => new QuestionnaireCategoricalOption
                        {
                            Value = option.Id,
                            ParentValue = option.ParentId != null ? (int)option.ParentId.Value : (int?)null,
                            Title = option.Text
                        });

            var isReadonly = IsReadOnly(id);

            return new EditOptionsViewModel
            (
                questionnaireId: id.QuestionnaireId.FormatGuid(),
                categoriesId: categoriesId,
                categoriesName: categoriesView.Name,
                options: categories.ToList(),
                isCascading: false,
                isCategories: true,
                isReadonly: isReadonly
            );
        }

        private bool IsReadOnly(QuestionnaireRevision questionnaireRevision)
        {
            if (questionnaireRevision.Revision != null)
                return true;
            
            if (questionnaireRevision.OriginalQuestionnaireId.HasValue)
                return true;

            var userId = User.GetIdOrNull();
            if (!userId.HasValue)
                return true;

            var hasUserAccessToEdit = questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(questionnaireRevision.OriginalQuestionnaireId ?? questionnaireRevision.QuestionnaireId, userId.Value);
            return !hasUserAccessToEdit;
        }

        public EditOptionsViewModel GetOptions(QuestionnaireRevision id, Guid questionId, bool isCascading)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            var options = editQuestionView != null
                ? editQuestionView.Options.Select(
                              option => new QuestionnaireCategoricalOption
                              {
                                  Value = option.Value != null ? (int)option.Value : throw new InvalidOperationException("Option Value must be not null."),
                                  ParentValue = option.ParentValue != null ? (int)option.ParentValue.Value : (int?)null,
                                  Title = option.Title
                              })
                : new QuestionnaireCategoricalOption[0];
            
            var isReadonly = IsReadOnly(id);

            return new EditOptionsViewModel
            (
                questionnaireId: id.QuestionnaireId.FormatGuid(),
                questionId: questionId,
                questionTitle: editQuestionView?.Title,
                options: options.ToList(),
                isCascading: isCascading,
                isReadonly: isReadonly
            )
            {
                CascadeFromQuestionId = editQuestionView?.CascadeFromQuestionId
            };
        }
        
        [HttpGet]
        [VuePage]
        public IActionResult EditOptions()
        {
            return Ok();
        }

        [HttpPost]
        public ActionResult<EditOptionsResponse> EditOptions([FromForm] QuestionnaireRevision id, [FromForm] Guid questionId, [FromForm] IFormFile? csvFile)
        {
            List<string> errors = new List<string>();

            if (csvFile == null)
            {
                errors.Add(Resources.QuestionnaireController.SelectTabFile);
                return new EditOptionsResponse
                {
                    Errors = errors
                };
            }

            try
            {
                var importResult = this.categoricalOptionsImportService.ImportOptions(
                    csvFile.OpenReadStream(), id.ToString(), questionId);

                if (importResult.Succeeded)
                {
                    return new EditOptionsResponse
                    {
                        Options = importResult.ImportedOptions.Select(c => new Category
                        {
                            ParentValue = c.ParentValue,
                            Title = c.Title,
                            Value = c.Value
                        }).ToArray()
                    };
                }
                else
                    errors.AddRange(importResult.Errors);
            }
            catch (Exception e)
            {
                errors.Add(Resources.QuestionnaireController.TabFilesOnly);
                this.logger.LogError(e, e.Message);
            }

            return new EditOptionsResponse
            {
                Errors = errors
            };
        }

        [HttpGet]
        [VuePage]
        public IActionResult EditCategories()
        {
            return Ok();
        }
        
        [HttpPost]
        public ActionResult<EditOptionsResponse> EditCategories(IFormFile? csvFile)
        {
            List<string> errors = new List<string>();

            if (csvFile == null)
            {
                errors.Add(Resources.QuestionnaireController.SelectTabFile);
                return new EditOptionsResponse
                {
                    Errors = errors
                };
            }

            try
            {
                var extension = this.fileSystemAccessor.GetFileExtension(csvFile.FileName);

                var excelExtensions = new[] { ".xlsx", ".ods", ".xls" };
                var tsvExtensions = new[] { ".txt", ".tab", ".tsv" };

                if (!excelExtensions.Union(tsvExtensions).Contains(extension))
                {
                    return new EditOptionsResponse
                    {
                        Errors = new List<string>()
                        {
                            ExceptionMessages.ImportOptions_Tab_Or_Excel_Only
                        }
                    };
                }

                var fileType = excelExtensions.Contains(extension)
                    ? CategoriesFileType.Excel
                    : CategoriesFileType.Tsv;

                var rows = this.categoriesService.GetRowsFromFile(csvFile.OpenReadStream(), fileType);

                return new EditOptionsResponse
                {
                    Options = rows
                        .Select(x => new Category()
                        {
                            Title = x.Text,
                            Value = int.Parse(x.Id!),
                            ParentValue = string.IsNullOrEmpty(x.ParentId)
                                ? (int?)null
                                : int.Parse(x.ParentId)
                        })
                        .ToArray()
                };
            }
            catch (InvalidFileException e)
            {
                var sb = new StringBuilder();
                sb.AppendLine(e.Message);
                e.FoundErrors?.ForEach(x => sb.AppendLine(x.Message));

                errors.Add(sb.ToString());
            }
            catch (ArgumentException ex)
            {
                errors.Add(ex.Message);
            }
            catch (Exception e)
            {
                errors.Add(Resources.QuestionnaireController.TabFilesOnly);
                this.logger.LogError(e, e.Message);
            }

            return this.Json(new { errors });
        }


        public class Category
        {
            public int Value { set; get; }
            public int? ParentValue { set; get; }
            public string Title { set; get; } = String.Empty;
        }

        public class UpdateCategoriesModel
        {
            public Category[]? Categories { set; get; }
        }

        public class EditOptionsResponse
        {
            public Category[]? Options { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }

        [HttpPost]
        public async Task<IActionResult?> ApplyOptions(QuestionnaireRevision id, Guid entityId,
            bool isCascading, bool isCategory,
            [FromBody] UpdateCategoriesModel? categoriesModel)
        {
            if (categoriesModel?.Categories == null)
                return Json(GetNotFoundResponseObject());

            if (isCategory)
            {
                var questionnaireId = id.QuestionnaireId;
                var categoriesId = Guid.NewGuid();

                //remove double parse
                try
                {
                    this.categoriesService.Store(questionnaireId,
                        categoriesId, categoriesModel.Categories.Select((x, i) => new CategoriesRow()
                        {
                            Id = x.Value.ToString(),
                            Text = x.Title,
                            ParentId = x.ParentValue == null ? "" : x.ParentValue.ToString(),
                            RowId = i
                        }).ToList());
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "Error on categories saving", e);

                    dynamic commandResult = new ExpandoObject();
                    commandResult.IsSuccess = false;
                    commandResult.Error = "Error occurred: " + e.Message;

                    return Json(commandResult);
                }

                var model = this.GetCategoryOptions(id, entityId);

                if (model.Value == null)
                {
                    return model.Result;
                }

                var command = new AddOrUpdateCategories(
                    questionnaireId,
                    this.User.GetId(),
                    categoriesId,
                    model.Value.CategoriesName ?? "",
                    model.Value.CategoriesId);

                var categoriesCommandResult = await this.ExecuteCommand(command);
                return Json(categoriesCommandResult);
            }
            else
            {
                var questionnaireCategoricalOptions = categoriesModel.Categories.Select(x => new QuestionnaireCategoricalOption()
                {
                    Value = x.Value,
                    ParentValue = x.ParentValue,
                    Title = x.Title
                }).ToArray();

                var command = isCascading
                    ? (QuestionCommand)new UpdateCascadingComboboxOptions(
                        id.QuestionnaireId,
                        entityId,
                        this.User.GetId(),
                        questionnaireCategoricalOptions)
                    : new UpdateFilteredComboboxOptions(
                        id.QuestionnaireId,
                        entityId,
                        this.User.GetId(),
                        questionnaireCategoricalOptions);

                var commandResult = await this.ExecuteCommand(command);

                return Json(commandResult);
            }
        }

        private object GetNotFoundResponseObject()
        {
            dynamic commandResult = new ExpandoObject();
            commandResult.IsSuccess = false;
            commandResult.Error = "Not Found";

            return commandResult;
        }

        private async Task<object> ExecuteCommand(QuestionnaireCommand command)
        {
            dynamic commandResult = new ExpandoObject();
            commandResult.IsSuccess = true;
            try
            {
                this.commandService.Execute(command);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.logger.LogError(e, "Error on command of type {type} handling", command.GetType());
                }

                commandResult = new ExpandoObject();
                commandResult.IsSuccess = false;
                commandResult.HasPermissions = domainEx != null && domainEx.ErrorType != DomainExceptionType.DoesNotHavePermissionsForEdit;
                commandResult.Error = domainEx != null ? domainEx.Message : "Something went wrong";
            }
            return commandResult;
        }

        public IActionResult ExportLookupTable(Guid id, Guid lookupTableId)
        {
            var lookupTableContentFile = this.lookupTableService.GetLookupTableContentFile(id, lookupTableId);
            if (lookupTableContentFile == null)
                return NotFound();

            return File(lookupTableContentFile.Content, "text/csv", lookupTableContentFile.FileName);
        }

        public IActionResult ExportOptions(QuestionnaireRevision id, Guid entityId, bool isCategory, bool isCascading)
        {
            if (isCategory)
            {
                var categoriesFile = this.categoriesService.GetAsExcelFile(id, entityId);

                if (categoriesFile?.Content == null) return NotFound();

                var categoriesName = string.IsNullOrEmpty(categoriesFile.CategoriesName)
                    ? "New categories"
                    : categoriesFile.CategoriesName;

                var filename = this.fileSystemAccessor.MakeValidFileName($"[{categoriesName}]{categoriesFile.QuestionnaireTitle}");

                return File(categoriesFile.Content,
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{filename}.xlsx");
            }

            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, entityId);

            var title = editQuestionView?.Title ?? "";
            var fileDownloadName = this.fileSystemAccessor.MakeValidFileName($"Options-in-question-{title}.txt");

            var export = this.categoricalOptionsImportService.ExportOptions(id.QuestionnaireId.FormatGuid(), entityId);

            return File(export, "text/csv", fileDownloadName);
        }

        public class EditOptionsViewModel
        {
            public EditOptionsViewModel(string questionnaireId,
                Guid? questionId = null,
                Guid? categoriesId = null,
                List<QuestionnaireCategoricalOption>? options = null,
                string? questionTitle = null,
                bool? isCascading = null,
                bool? isCategories = null,
                string? categoriesName = null,
                bool? isReadonly = true)
            {
                if (questionId == null && categoriesId == null)
                    throw new InvalidOperationException($"{nameof(categoriesId)} or {nameof(questionId)} should not be empty");

                QuestionnaireId = questionnaireId;
                if (questionId != null)
                    QuestionId = questionId.Value;

                if (categoriesId != null)
                    CategoriesId = categoriesId.Value;
                if (categoriesName != null)
                    CategoriesName = categoriesName;

                Options = options ?? new List<QuestionnaireCategoricalOption>();
                QuestionTitle = questionTitle;
                IsCascading = isCascading ?? false;
                IsCategories = isCategories ?? false;
                IsReadonly = isReadonly ?? true;
            }

            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }

            public string? CategoriesName { get; set; }
            public Guid CategoriesId { get; set; }

            public List<QuestionnaireCategoricalOption> Options { get; set; }
            public string? QuestionTitle { get; set; }
            public bool IsCascading { get; set; }

            public bool IsCategories { get; set; }
            public string? CascadeFromQuestionId { get; set; }
            public bool IsReadonly { get; set; }
        }
    }
}
