﻿using System;
using System.Dynamic;
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
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        #region [Edit options]
        private const string OptionsSessionParameterName = "options";

        public EditOptionsViewModel? questionWithOptionsViewModel
        {
            get
            {
                var model = this.HttpContext.Session.Get(OptionsSessionParameterName);
                if (model != null)
                {
                    return JsonConvert.DeserializeObject<EditOptionsViewModel>(Encoding.UTF8.GetString(model));
                }

                return null;
            }
            set
            {
                string data = JsonConvert.SerializeObject(value);

                this.HttpContext.Session.Set(OptionsSessionParameterName, Encoding.UTF8.GetBytes(data));
            }
        }

        public IActionResult EditOptions(QuestionnaireRevision id, Guid questionId)
        {
            this.SetupViewModel(id, questionId);
            if (this.questionWithOptionsViewModel == null)
                return NotFound();

            return this.View(this.questionWithOptionsViewModel);
        }

        public IActionResult EditCascadingOptions(QuestionnaireRevision id, Guid questionId)
            => this.EditOptions(id, questionId);

        private void SetupViewModel(QuestionnaireRevision id, Guid questionId)
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

            this.questionWithOptionsViewModel = new EditOptionsViewModel(
                id.QuestionnaireId.FormatGuid(),
                questionId,
                options.ToArray(),
                editQuestionView?.Title
            );
        }

        public IActionResult ResetOptions()
        {
            if (this.questionWithOptionsViewModel == null)
                return NotFound();

            return RedirectToAction("EditOptions",
                new
                {
                    id = this.questionWithOptionsViewModel.QuestionnaireId,
                    questionId = this.questionWithOptionsViewModel.QuestionId
                });
        }

        public IActionResult ResetCascadingOptions()
        {
            if (this.questionWithOptionsViewModel == null)
                return NotFound();

            return RedirectToAction("EditCascadingOptions",
                new
                {
                    id = this.questionWithOptionsViewModel.QuestionnaireId,
                    questionId = this.questionWithOptionsViewModel.QuestionId
                });
        }

        [HttpPost]
        public IActionResult EditOptions(IFormFile csvFile)
        {
            var withOptionsViewModel = this.questionWithOptionsViewModel;
            if (withOptionsViewModel == null)
            {
                this.Error(Resources.QuestionnaireController.Error);
                return this.View(questionWithOptionsViewModel);
            }

            if (csvFile == null)
            {
                this.Error(Resources.QuestionnaireController.SelectTabFile);
                return this.View(questionWithOptionsViewModel);
            }

            try
            {
                var importResult = this.categoricalOptionsImportService.ImportOptions(
                    csvFile.OpenReadStream(),
                    withOptionsViewModel.QuestionnaireId,
                    withOptionsViewModel.QuestionId);

                if (importResult.Succeeded)
                {
                    withOptionsViewModel.Options = importResult.ImportedOptions.ToArray();
                }
                else
                {
                    foreach (var importError in importResult.Errors)
                        this.Error(importError, true);
                }

                this.questionWithOptionsViewModel = withOptionsViewModel;
            }
            catch (Exception e)
            {
                this.Error(Resources.QuestionnaireController.TabFilesOnly);
                this.logger.LogError(e, e.Message);
            }

            return this.View(questionWithOptionsViewModel);
        }

        [HttpPost]
        public IActionResult EditCascadingOptions(IFormFile csvFile)
            => this.EditOptions(csvFile);

        [HttpPost]
        public async Task<IActionResult> ApplyOptions()
        {
            if (this.questionWithOptionsViewModel == null)
                return Json(GetNotFoundResponseObject());

            var questionnaireCategoricalOptions = this.questionWithOptionsViewModel.Options.ToArray();
            var commandResult = await this.ExecuteCommand(
                new UpdateFilteredComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.User.GetId(),
                        questionnaireCategoricalOptions));

            return Json(commandResult);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCascadingOptions()
        {
            if (this.questionWithOptionsViewModel == null)
                return Json(GetNotFoundResponseObject());

            var commandResult = await this.ExecuteCommand(
                new UpdateCascadingComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.User.GetId(),
                        this.questionWithOptionsViewModel.Options.ToArray()));

            return Json(commandResult);
        }

        private object GetNotFoundResponseObject()
        {
            dynamic commandResult = new ExpandoObject();
            commandResult.IsSuccess = false;
            commandResult.Error = "Not Found";

            return commandResult;
        }



        private async Task<object> ExecuteCommand(QuestionCommand command)
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
                commandResult.Error = domainEx != null ? domainEx.Message : "Something goes wrong";
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

            var title = this.questionWithOptionsViewModel.QuestionTitle ?? "";
            var fileDownloadName = this.fileSystemAccessor.MakeValidFileName($"Options-in-question-{title}.txt");

            return File(this.categoricalOptionsImportService.ExportOptions(
                this.questionWithOptionsViewModel.QuestionnaireId,
                this.questionWithOptionsViewModel.QuestionId), "text/csv", fileDownloadName);
        }

        public class EditOptionsViewModel
        {
            public EditOptionsViewModel(string questionnaireId, Guid questionId, QuestionnaireCategoricalOption[]? options = null, string? questionTitle = null)
            {
                QuestionnaireId = questionnaireId;
                QuestionId = questionId;
                Options = options ?? new QuestionnaireCategoricalOption[0];
                QuestionTitle = questionTitle;
            }

            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }
            public QuestionnaireCategoricalOption[] Options { get; set; }
            public string? QuestionTitle { get; set; }
        }

        #endregion
    }
}
