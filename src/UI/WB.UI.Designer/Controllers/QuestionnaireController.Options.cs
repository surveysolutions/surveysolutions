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
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        #region [Edit options]
        private const string OptionsSessionParameterName = "options";

        public EditOptionsViewModel questionWithOptionsViewModel
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

        public IActionResult EditOptions(QuestionnaireRevision id, Guid questionId, bool? isCascading = false)
        {
            this.SetupViewModel(id, questionId, isCascading ?? false);
            return this.View("EditOptions", this.questionWithOptionsViewModel);
        }
        public IActionResult EditCascadingOptions(QuestionnaireRevision id, Guid questionId)
            => this.EditOptions(id, questionId, true);
        
        public IActionResult GetOptions() => this.Json(this.questionWithOptionsViewModel?.Options);

        private void SetupViewModel(QuestionnaireRevision id, Guid questionId, bool isCascading)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            var options = editQuestionView?.Options.Select(
                              option => new QuestionnaireCategoricalOption
                              {
                                  Value = (int)option.Value,
                                  ParentValue = (int?)option.ParentValue,
                                  Title = option.Title
                              }) ??
                          new QuestionnaireCategoricalOption[0];

            this.questionWithOptionsViewModel = new EditOptionsViewModel
            {
                QuestionnaireId = id.QuestionnaireId.FormatGuid(),
                QuestionId = questionId,
                QuestionTitle = editQuestionView.Title,
                Options = options.ToArray(),
                IsCascading = isCascading
            };
        }

        public IActionResult ResetOptions()
        {
            this.SetupViewModel(
                new QuestionnaireRevision(this.questionWithOptionsViewModel.QuestionnaireId),
                this.questionWithOptionsViewModel.QuestionId,
                this.questionWithOptionsViewModel.IsCascading);

            return this.Ok();
            
        }

        [HttpPost]
        public IActionResult EditOptions(IFormFile csvFile)
        {
            List<string> errors = new List<string>();
            
            var withOptionsViewModel = this.questionWithOptionsViewModel;
            if (csvFile == null)
                errors.Add(Resources.QuestionnaireController.SelectTabFile);
            else
            {
                try
                {
                    var importResult = this.categoricalOptionsImportService.ImportOptions(
                        csvFile.OpenReadStream(),
                        withOptionsViewModel.QuestionnaireId,
                        withOptionsViewModel.QuestionId);

                    if (importResult.Succeeded)
                        withOptionsViewModel.Options = importResult.ImportedOptions.ToArray();
                    else
                        errors.AddRange(importResult.Errors);
                }
                catch (Exception e)
                {
                    errors.Add(Resources.QuestionnaireController.TabFilesOnly);
                    this.logger.LogError(e, e.Message);
                }
            }

            this.questionWithOptionsViewModel = withOptionsViewModel;

            return this.Json(errors);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyOptions()
        {
            var questionnaireCategoricalOptions = this.questionWithOptionsViewModel.Options.ToArray();

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
            return File(lookupTableContentFile.Content, "text/csv", lookupTableContentFile.FileName);
        }

        public IActionResult ExportOptions()
        {
            var title = this.questionWithOptionsViewModel.QuestionTitle ?? "";
            var fileDownloadName = this.fileSystemAccessor.MakeValidFileName($"Options-in-question-{title}.txt");

            return File(this.categoricalOptionsImportService.ExportOptions(
                this.questionWithOptionsViewModel.QuestionnaireId,
                this.questionWithOptionsViewModel.QuestionId), "text/csv", fileDownloadName);
        }

        public class EditOptionsViewModel
        {
            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }
            public QuestionnaireCategoricalOption[] Options { get; set; }
            public string QuestionTitle { get; set; }
            public bool IsCascading { get; set; }
        }

        #endregion
    }
}
