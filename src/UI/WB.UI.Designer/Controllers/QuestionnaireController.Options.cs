using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        private const string OptionsSessionParameterName = "options";

        private EditOptionsViewModel? _questionWithOptionsViewModel = null;

        public EditOptionsViewModel? questionWithOptionsViewModel
        {
            get
            {
                if (_questionWithOptionsViewModel != null) return _questionWithOptionsViewModel;
                var model = this.HttpContext.Session.Get(OptionsSessionParameterName);
                if (model != null)
                {
                    _questionWithOptionsViewModel = JsonConvert.DeserializeObject<EditOptionsViewModel>(Encoding.UTF8.GetString(model));
                    return _questionWithOptionsViewModel;
                }

                return null;
            }
            set
            {
                _questionWithOptionsViewModel = value;
                string data = JsonConvert.SerializeObject(value);

                this.HttpContext.Session.Set(OptionsSessionParameterName, Encoding.UTF8.GetBytes(data));
            }
        }

        public IActionResult GetCategoryOptions(QuestionnaireRevision id, Guid categoriesId)
        {
            var categoriesView = this.questionnaireInfoFactory.GetCategoriesView(id, categoriesId);

            if (categoriesView == null)
                return NotFound();

            var categories =
                this.categoriesService.GetCategoriesById(id.QuestionnaireId, categoriesId).
                    Select(
                        option => new QuestionnaireCategoricalOption
                        {
                            Value = option.Id,
                            ParentValue = option.ParentId != null ? (int)option.ParentId.Value : (int?)null,
                            Title = option.Text
                        });

            this.questionWithOptionsViewModel = new EditOptionsViewModel
            (
                questionnaireId: id.QuestionnaireId.FormatGuid(),
                categoriesId: categoriesId,
                categoriesName: categoriesView.Name,
                options: categories.ToList(),
                isCascading: true,
                isCategories: true
            );

            if (this.questionWithOptionsViewModel == null)
                return NotFound();

            return this.Json(this.questionWithOptionsViewModel);
        }

        public IActionResult GetOptions(QuestionnaireRevision id, Guid questionId, bool isCascading)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);
            
            var options = editQuestionView != null 
                ? editQuestionView.Options.Select(
                              option => new QuestionnaireCategoricalOption
                              {
                                  Value = option.Value != null ? (int)option.Value : throw new InvalidOperationException("Option Value must be not null."),
                                  ParentValue = option.ParentValue != null ? (int)option.ParentValue.Value : (int?) null,
                                  Title = option.Title
                              }) 
                : new QuestionnaireCategoricalOption[0];

            this.questionWithOptionsViewModel = new EditOptionsViewModel
            (
                questionnaireId : id.QuestionnaireId.FormatGuid(),
                questionId : questionId,
                questionTitle : editQuestionView?.Title,
                options : options.ToList(),
                isCascading : isCascading
            );

            return Json(this.questionWithOptionsViewModel);
        }

        public IActionResult ResetOptions()
        {
            this.questionWithOptionsViewModel = null;
            return Ok();
        }

        [HttpPost]
        public IActionResult EditOptions(IFormFile? csvFile)
        {
            List<string> errors = new List<string>();
            
            var withOptionsViewModel = this.questionWithOptionsViewModel;
            if (withOptionsViewModel == null)
            {
                errors.Add(Resources.QuestionnaireController.Error);
                return this.Json(errors);
            }

            if (csvFile == null)
            {
                errors.Add(Resources.QuestionnaireController.SelectTabFile);
                return this.Json(errors);
            }

            try
            {
                var importResult = this.categoricalOptionsImportService.ImportOptions(
                    csvFile.OpenReadStream(),
                    withOptionsViewModel.QuestionnaireId,
                    withOptionsViewModel.QuestionId);

                if (importResult.Succeeded)
                {
                    withOptionsViewModel.Options = importResult.ImportedOptions.ToList();
                    this.questionWithOptionsViewModel = withOptionsViewModel;
                }
                else
                    errors.AddRange(importResult.Errors);
            }
            catch (Exception e)
            {
                errors.Add(Resources.QuestionnaireController.TabFilesOnly);
                this.logger.LogError(e, e.Message);
            }
            
            return this.Json(errors);
        }

        [HttpPost]
        public IActionResult EditCategories(IFormFile? csvFile)
        {
            List<string> errors = new List<string>();

            var withOptionsViewModel = this.questionWithOptionsViewModel;
            if (withOptionsViewModel == null)
            {
                errors.Add(Resources.QuestionnaireController.Error);
                return this.Json(errors);
            }

            if (csvFile == null)
            {
                errors.Add(Resources.QuestionnaireController.SelectTabFile);
                return this.Json(errors);
            }

            try
            {
                var extension = this.fileSystemAccessor.GetFileExtension(csvFile.FileName);

                var excelExtensions = new[] {".xlsx", ".ods", ".xls"};
                var tsvExtensions = new[] {".txt", ".tab", ".tsv"};

                if (!excelExtensions.Union(tsvExtensions).Contains(extension))
                    throw new ArgumentException(ExceptionMessages.ImportOptions_Tab_Or_Excel_Only);

                var fileType = excelExtensions.Contains(extension)
                    ? CategoriesFileType.Excel
                    : CategoriesFileType.Tsv;

                var rows = this.categoriesService.GetRowsFromFile(csvFile.OpenReadStream(), fileType);

                withOptionsViewModel.Options =
                    rows
                        .Select(x => new QuestionnaireCategoricalOption()
                        {
                            Title = x.Text,
                            Value = int.Parse(x.Id!),
                            ParentValue = string.IsNullOrEmpty(x.ParentId)
                                ? (int?) null
                                : int.Parse(x.ParentId)
                        })
                        .ToList();

                this.questionWithOptionsViewModel = withOptionsViewModel;
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
            
            return this.Json(errors);
        }

        [HttpPost]
        public IActionResult EditCascadingOptions(IFormFile csvFile)
            => this.EditOptions(csvFile);

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

        [HttpPost]
        public async Task<IActionResult> ApplyOptions([FromBody]UpdateCategoriesModel? categoriesModel)
        {
            if(categoriesModel?.Categories == null)
                return Json(GetNotFoundResponseObject());

            if (this.questionWithOptionsViewModel == null)
                return Json(GetNotFoundResponseObject());
            
            if (this.questionWithOptionsViewModel.IsCategories)
            {
                var questionnaireId = Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId);
                var categoriesId = Guid.NewGuid();
                
                //remove double parse
                try
                {
                    this.categoriesService.Store(questionnaireId,
                        categoriesId, categoriesModel.Categories.Select((x,i) => new CategoriesRow()
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

                var command = new AddOrUpdateCategories(
                    questionnaireId,
                    this.User.GetId(),
                    categoriesId,
                    this.questionWithOptionsViewModel.CategoriesName ?? "",
                    this.questionWithOptionsViewModel.CategoriesId);

                var categoriesCommandResult = await this.ExecuteCommand(command);
                return Json(categoriesCommandResult);
            }
            else
            {
                var questionnaireCategoricalOptions = categoriesModel.Categories.Select(x=> new QuestionnaireCategoricalOption()
                {
                    Value = x.Value,
                    ParentValue = x.ParentValue,
                    Title = x.Title
                }).ToArray();

                var command = this.questionWithOptionsViewModel.IsCascading
                    ? (QuestionCommand) new UpdateCascadingComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.User.GetId(),
                        questionnaireCategoricalOptions)
                    : new UpdateFilteredComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.User.GetId(),
                        questionnaireCategoricalOptions);

                var commandResult = await this.ExecuteCommand(command);

                return Json(commandResult);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCascadingOptions([FromBody] UpdateCategoriesModel? categoriesModel)
        {
            if (this.questionWithOptionsViewModel == null || categoriesModel == null)
                return Json(GetNotFoundResponseObject());

            var commandResult = await this.ExecuteCommand(
                new UpdateCascadingComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.User.GetId(),
                        categoriesModel.Categories!.Select(c => new QuestionnaireCategoricalOption
                        {
                            Title = c.Title,
                            Value = c.Value,
                            ParentValue = c.ParentValue
                        }).ToArray()));

            return Json(commandResult);
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

        public IActionResult ExportOptions()
        {
            if (this.questionWithOptionsViewModel == null)
                return NotFound();
            
            if (this.questionWithOptionsViewModel.IsCategories)
            {
                var categoriesFile = this.categoriesService.GetAsExcelFile(this.questionWithOptionsViewModel.QuestionId, this.questionWithOptionsViewModel.CategoriesId);

                if (categoriesFile?.Content == null) return NotFound();

                var categoriesName = string.IsNullOrEmpty(categoriesFile.CategoriesName)
                    ? "New categories"
                    : categoriesFile.CategoriesName;

                var filename = this.fileSystemAccessor.MakeValidFileName($"[{categoriesName}]{categoriesFile.QuestionnaireTitle}");

                return File(categoriesFile.Content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{filename}.xlsx");
            }

            var title = this.questionWithOptionsViewModel.QuestionTitle ?? "";
            var fileDownloadName = this.fileSystemAccessor.MakeValidFileName($"Options-in-question-{title}.txt");

            return File(this.categoricalOptionsImportService.ExportOptions(
                this.questionWithOptionsViewModel.QuestionnaireId,
                this.questionWithOptionsViewModel.QuestionId), "text/csv", fileDownloadName);
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
                string? categoriesName = null)
            {
                if(questionId == null && categoriesId == null)
                    throw new InvalidOperationException($"{nameof(categoriesId)} or {nameof(questionId)} should not be empty");

                QuestionnaireId = questionnaireId;
                if(questionId !=null)
                    QuestionId = questionId.Value;

                if (categoriesId != null)
                    CategoriesId = categoriesId.Value;
                if (categoriesName != null)
                    CategoriesName = categoriesName;

                Options = options ?? new List<QuestionnaireCategoricalOption>();
                QuestionTitle = questionTitle;
                IsCascading = isCascading ?? false;
                IsCategories = isCategories ?? false;
            }

            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }

            public string? CategoriesName { get; set; }
            public Guid CategoriesId { get; set; }

            public List<QuestionnaireCategoricalOption> Options { get; set; }
            public string? QuestionTitle { get; set; }
            public bool IsCascading { get; set; }

            public bool IsCategories { get; set; }
        }
    }
}
